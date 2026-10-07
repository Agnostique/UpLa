<?php

/*
 * UpLa desktop app sign-in for upla.com.tr.
 * A Chevereto 4.5.x route override: copy to <chevereto>/app/legacy/routes/overrides/upla-app.php
 * (no core file is changed). Part of the UpLa desktop app, licensed under GPL v3; see Server/README.md.
 *
 * JSON endpoints, POST only, the "X-Upla-App: 1" header is required (browsers cannot send it cross-site):
 *   /upla-app/login   login-subject, password, [two-factor-code], [device]
 *                     -> a new API key for that computer, stored as "UpLa - <device>"
 *   /upla-app/me      X-API-Key header -> the account the key belongs to
 *   /upla-app/logout  X-API-Key header -> deletes that key
 * Web page for signed-in members:
 *   /upla-app/devices -> lists the account's API keys, removes them and creates website keys
 *
 * The app cannot show the site's CAPTCHA, so sign-in attempts are limited per IP and per account. Failed sign-ins are
 * recorded like website login failures: website and app failures share the limits and Chevereto's own daily lockout.
 */

use Chevereto\Legacy\Classes\ApiKey;
use Chevereto\Legacy\Classes\DB;
use Chevereto\Legacy\Classes\L10n;
use Chevereto\Legacy\Classes\Login;
use Chevereto\Legacy\Classes\RequestLog;
use Chevereto\Legacy\Classes\TwoFactor;
use Chevereto\Legacy\Classes\User;
use Chevereto\Legacy\G\Handler;
use function Chevereto\Legacy\decodeID;
use function Chevereto\Legacy\encodeID;
use function Chevereto\Legacy\G\get_base_url;
use function Chevereto\Legacy\G\get_client_ip;
use function Chevereto\Legacy\G\get_current_url;
use function Chevereto\Legacy\G\redirect;
use function Chevereto\Legacy\getSetting;
use function Chevereto\Legacy\headersNoCache;
use function Chevereto\Legacy\passwordHash;
use function Chevereto\Vars\env;
use function Chevereto\Vars\post;
use function Chevereto\Vars\server;
use function Chevereto\Vars\session;
use function Chevereto\Vars\sessionVar;

return function (Handler $handler) {
    $appKeyPrefix = 'UpLa - ';
    $maxAppKeysPerUser = 10;
    $maxWebsiteKeysPerUser = 5;
    $maxIpFailuresPerHour = 5;
    $maxIpFailuresPerDay = 10;
    $maxUserFailuresPerDay = 10;
    $failureTypes = ['login', 'account-two-factor'];

    $request = $handler->request();
    $action = (string) ($request[0] ?? '');
    $method = strtoupper((string) (server()['REQUEST_METHOD'] ?? 'GET'));

    if (count($request) !== 1 || ! in_array($action, ['login', 'me', 'logout', 'devices'], true)) {
        $handler->issueError(404);

        return;
    }
    // Nothing here may be cached (Chevereto adds Cache-Control max-age to routes when the Dashboard cache time is set).
    headersNoCache();

    // Own JSON output: Chevereto's json_document_output() only knows some status codes and throws on 429.
    $respond = function (int $status, array $data): void {
        $texts = [200 => 'OK', 400 => 'Bad Request', 401 => 'Unauthorized', 403 => 'Forbidden', 405 => 'Method Not Allowed',
            429 => 'Too Many Requests', 500 => 'Internal Server Error'];
        headersNoCache();
        header('Content-Type: application/json; charset=UTF-8');
        header('X-Content-Type-Options: nosniff');
        if (isset($data['retry_after'])) {
            header('Retry-After: ' . (int) $data['retry_after']);
        }
        http_response_code($status);
        echo json_encode(['status_code' => $status, 'status_txt' => $texts[$status] ?? ''] + $data, JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE);
        exit();
    };
    $fail = function (int $status, string $code, string $message, array $extra = []) use ($respond): void {
        $respond($status, ['error' => ['code' => $code, 'message' => $message]] + $extra);
    };
    $publicUser = function (array $user): array {
        return [
            'username' => (string) ($user['username'] ?? ''),
            'name' => (string) ($user['name'] ?? ''),
            'url' => (string) ($user['url'] ?? ''),
        ];
    };
    $accountError = function (string $status) use ($fail): bool {
        switch ($status) {
            case 'valid':
                return false;
            case 'banned':
                $fail(403, 'account_banned', 'This account is banned.');

                return true;
            case 'awaiting-confirmation':
                $fail(403, 'account_awaiting_confirmation', 'This account is waiting for email confirmation.');

                return true;
            case 'awaiting-email':
                $fail(403, 'account_awaiting_email', 'This account needs an email address.');

                return true;
            default:
                $fail(403, 'account_not_valid', 'This account cannot sign in.');

                return true;
        }
    };
    // Returns the key row (id, user_id, date_gmt) for the X-API-Key header, or [] when it is missing or invalid.
    $verifyKey = function (): array {
        $key = trim((string) (server()['HTTP_X_API_KEY'] ?? ''));
        if ($key === '') {
            return [];
        }
        try {
            return ApiKey::verify($key);
        } catch (Throwable) {
            return [];
        }
    };
    $isTurkish = function (): bool {
        try {
            return str_starts_with(strtolower(L10n::getLocale()), 'tr');
        } catch (Throwable) {
            return true;
        }
    };

    if ($action === 'devices') {
        $logged_user = Login::getUser();
        if ($logged_user === []) {
            sessionVar()->put('last_url', get_current_url());
            redirect('login', 302);

            return;
        }
        if (($logged_user['status'] ?? '') !== 'valid') {
            $handler->issueError(403);

            return;
        }
        $userId = (int) $logged_user['id'];
        if ($method === 'POST') {
            if (! $handler::checkAuthToken((string) (post()['auth_token'] ?? ''))) {
                $handler->issueError(403);

                return;
            }
            if ((string) (post()['action'] ?? '') === 'create') {
                $websiteKeys = DB::fetchSingleQuery(
                    'SELECT COUNT(*) AS total FROM `' . DB::getTable('api_keys') . '`'
                    . ' WHERE api_key_user_id=:user_id AND (api_key_name IS NULL OR api_key_name NOT LIKE :prefix)',
                    [':user_id' => $userId, ':prefix' => $appKeyPrefix . '%']
                );
                if ((int) ($websiteKeys['total'] ?? 0) >= $maxWebsiteKeysPerUser) {
                    sessionVar()->put('upla_app_notice', 'website_key_limit');
                } else {
                    // Shown once on the next page load, like Chevereto's own Settings > API page.
                    sessionVar()->put('upla_app_new_key', ApiKey::insert($userId));
                }
            } else {
                $keyId = 0;
                try {
                    $keyId = decodeID((string) (post()['key'] ?? ''));
                } catch (Throwable) {
                }
                if ($keyId > 0) {
                    $row = DB::get('api_keys', ['id' => $keyId, 'user_id' => $userId], 'AND', [], 1);
                    if ($row) {
                        ApiKey::remove($keyId);
                    }
                }
            }
            redirect('upla-app/devices', 303);

            return;
        }
        $tr = $isTurkish();
        $t = fn (string $turkish, string $english): string => $tr ? $turkish : $english;
        $e = fn (string $text): string => htmlspecialchars($text, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
        $authToken = $e($handler::getAuthToken());
        $formAction = $e(get_base_url('upla-app/devices'));
        $newKey = (string) (session()['upla_app_new_key'] ?? '');
        $notice = (string) (session()['upla_app_notice'] ?? '');
        foreach (['upla_app_new_key', 'upla_app_notice'] as $flash) {
            if (sessionVar()->has($flash)) {
                sessionVar()->remove($flash);
            }
        }
        $rows = DB::get('api_keys', ['user_id' => $userId], 'AND', ['field' => 'id', 'order' => 'desc']) ?: [];
        $items = '';
        foreach ($rows as $row) {
            $key = DB::formatRow($row, 'api_key');
            $name = (string) ($key['name'] ?? '');
            $label = $name !== '' ? $name : $t('Web sitesi API anahtarı', 'Website API key');
            $encoded = encodeID((int) $key['id']);
            $items .= '<tr><td>' . $e($label) . '<br><small>chv_' . $e($encoded) . '_***</small></td>'
                . '<td>' . $e((string) $key['date_gmt']) . ' UTC</td>'
                . '<td><form method="post" action="' . $formAction . '">'
                . '<input type="hidden" name="auth_token" value="' . $authToken . '">'
                . '<input type="hidden" name="key" value="' . $e($encoded) . '">'
                . '<button type="submit">' . $e($t('Kaldır', 'Remove')) . '</button></form></td></tr>';
        }
        if ($items === '') {
            $items = '<tr><td colspan="3">' . $e($t('Bağlı cihaz veya API anahtarı yok.', 'No connected devices or API keys.')) . '</td></tr>';
        }
        $newKeyBox = '';
        if ($newKey !== '') {
            $newKeyBox = '<div class="box"><p><strong>' . $e($t('Yeni API anahtarınız:', 'Your new API key:')) . '</strong></p>'
                . '<input readonly onclick="this.select()" value="' . $e($newKey) . '">'
                . '<p>' . $e($t('Bu anahtar yalnızca şimdi gösterilir; güvenli bir yere kopyalayın.', 'This key is shown only now; copy it to a safe place.')) . '</p></div>';
        } elseif ($notice === 'website_key_limit') {
            $newKeyBox = '<div class="box"><p>' . $e($t(
                "En fazla {$maxWebsiteKeysPerUser} web sitesi anahtarınız olabilir. Yeni bir tane oluşturmadan önce kullanmadığınız birini kaldırın.",
                "You can have at most {$maxWebsiteKeysPerUser} website keys. Remove one you do not use before creating a new one."
            )) . '</p></div>';
        }
        header('Content-Type: text/html; charset=UTF-8');
        header('X-Content-Type-Options: nosniff');
        echo '<!DOCTYPE html><html lang="' . ($tr ? 'tr' : 'en') . '"><head><meta charset="utf-8">'
            . '<meta name="viewport" content="width=device-width, initial-scale=1">'
            . '<title>' . $e($t('Bağlı cihazlar', 'Connected devices')) . ' - ' . $e((string) getSetting('website_name')) . '</title>'
            . '<style>body{font-family:system-ui,sans-serif;margin:2rem auto;max-width:46rem;padding:0 1rem;color:#222}'
            . 'table{border-collapse:collapse;width:100%}td{border-bottom:1px solid #ddd;padding:.6rem .4rem;vertical-align:top}'
            . 'small{color:#777}button{cursor:pointer}.box{background:#f3f7ff;border:1px solid #c9d8f5;padding:.2rem 1rem;margin:1rem 0}'
            . '.box input{width:100%;font-family:monospace;padding:.3rem}</style></head><body>'
            . '<h1>' . $e($t('Bağlı cihazlar', 'Connected devices')) . '</h1>'
            . '<p>' . $e($t(
                'UpLa uygulamasına giriş yaptığınız her bilgisayar ve oluşturduğunuz web sitesi API anahtarları burada listelenir. Kaldırdığınız cihaz hesabınıza yükleme yapamaz; uygulamada tekrar giriş yapmak gerekir. Şifrenizi değiştirmek bu bağlantıları kaldırmaz; kaybolan bir bilgisayarı buradan kaldırın.',
                'Every computer signed in to the UpLa app and the website API keys you created are listed here. A removed device can no longer upload to your account until it signs in again. Changing your password does not remove these connections; remove a lost computer here.'
            )) . '</p>'
            . $newKeyBox
            . '<table>' . $items . '</table>'
            . '<form method="post" action="' . $formAction . '"><input type="hidden" name="auth_token" value="' . $authToken . '">'
            . '<input type="hidden" name="action" value="create"><p><button type="submit">'
            . $e($t('Yeni API anahtarı oluştur', 'Create a new API key')) . '</button> <small>' . $e($t(
                'Başka programlar (ör. ShareX) için. Uygulamanın kendisi için gerekmez; uygulamadan giriş yapmanız yeterli.',
                'For other programs (e.g. ShareX). The app itself does not need one; signing in from the app is enough.'
            )) . '</small></p></form>'
            . '<p><small>' . $e($t(
                'Ayarlar › API sayfası yalnızca en yeni anahtarı gösterir ve oradaki "Regen key" o anahtarı siler; bu, giriş yaptığınız bir bilgisayarın bağlantısı olabilir. Anahtarları bu sayfadan yönetin.',
                'Settings › API shows only the newest key, and its "Regen key" deletes that key, which may be one of your signed-in computers. Manage keys on this page instead.'
            )) . '</small></p>'
            . '</body></html>';
        exit();
    }

    if ($method !== 'POST') {
        $fail(405, 'method_not_allowed', 'Use POST.');

        return;
    }
    if (trim((string) (server()['HTTP_X_UPLA_APP'] ?? '')) === '') {
        $fail(400, 'missing_app_header', 'The X-Upla-App header is required.');

        return;
    }

    try {
        if ($action === 'me' || $action === 'logout') {
            $verify = $verifyKey();
            if ($verify === []) {
                $fail(401, 'invalid_key', 'Invalid API key.');

                return;
            }
            if ($action === 'logout') {
                ApiKey::remove((int) $verify['id']);
                $respond(200, ['success' => ['message' => 'Signed out.']]);

                return;
            }
            $user = User::getSingle((int) $verify['user_id'], 'id', true);
            if ($user === []) {
                $fail(401, 'invalid_key', 'Invalid API key.');

                return;
            }
            if ($accountError((string) ($user['status'] ?? ''))) {
                return;
            }
            $respond(200, ['user' => $publicUser($user)]);

            return;
        }

        // login
        if (! getSetting('enable_api_user')) {
            $fail(403, 'api_disabled', 'API keys are disabled for members.');

            return;
        }
        $subject = trim((string) (post()['login-subject'] ?? ''));
        $password = (string) (post()['password'] ?? '');
        $code = preg_replace('/\s+/', '', (string) (post()['two-factor-code'] ?? '')) ?? '';
        $device = preg_replace('/[\p{C}]+/u', '', (string) (post()['device'] ?? '')) ?? '';
        $device = trim(mb_substr(trim($device), 0, 60));
        if ($subject === '' || $password === '') {
            $fail(400, 'missing_fields', 'Username or email and password are required.');

            return;
        }
        $tooMany = function (bool $day) use ($fail): void {
            $fail(429, 'too_many_attempts', 'Too many failed sign-in attempts. Try again later.', [
                'retry_after' => $day ? 86400 : 3600,
            ]);
        };
        // Cheap check first: no database writes for someone who is already over a limit.
        $ipFailures = RequestLog::getCounts($failureTypes, 'fail');
        if ((int) $ipFailures['hour'] >= $maxIpFailuresPerHour || (int) $ipFailures['day'] >= $maxIpFailuresPerDay) {
            $tooMany((int) $ipFailures['day'] >= $maxIpFailuresPerDay);

            return;
        }
        $loginBy = filter_var($subject, FILTER_VALIDATE_EMAIL) ? 'email' : 'username';
        $user = User::getSingle($subject, $loginBy, true);
        $userId = (int) ($user['id'] ?? 0);
        $ip = get_client_ip();
        // Record the attempt as a failure before the slow password check and count again, so parallel requests cannot
        // all pass the limits; the row is removed again when the password turns out to be right.
        $attemptId = (int) RequestLog::insert(['type' => 'login', 'user_id' => $userId > 0 ? $userId : null, 'result' => 'fail']);
        $counts = DB::fetchSingleQuery(
            'SELECT'
            . ' COUNT(IF(request_ip=:ip1 AND request_date_gmt >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL 1 HOUR), 1, NULL)) AS ip_hour,'
            . ' COUNT(IF(request_ip=:ip2, 1, NULL)) AS ip_day,'
            . ' COUNT(IF(request_user_id=:user_id1, 1, NULL)) AS user_day'
            . ' FROM `' . DB::getTable('requests') . '`'
            . " WHERE request_result='fail' AND request_type IN ('login', 'account-two-factor')"
            . ' AND request_date_gmt >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL 1 DAY)'
            . ' AND (request_ip=:ip3 OR request_user_id=:user_id2)',
            [':ip1' => $ip, ':ip2' => $ip, ':ip3' => $ip, ':user_id1' => $userId, ':user_id2' => $userId]
        );
        $ipDayOver = (int) ($counts['ip_day'] ?? 0) > $maxIpFailuresPerDay;
        $userDayOver = $userId > 0 && (int) ($counts['user_day'] ?? 0) > $maxUserFailuresPerDay;
        if ($ipDayOver || $userDayOver || (int) ($counts['ip_hour'] ?? 0) > $maxIpFailuresPerHour) {
            DB::delete('requests', ['id' => $attemptId]);
            $tooMany($ipDayOver || $userDayOver);

            return;
        }
        $valid = false;
        if ($userId > 0 && Login::hasPassword($userId)) {
            $valid = Login::checkPassword($userId, $password);
        } else {
            // Same work as a real password check, so the response time does not reveal whether the account exists.
            passwordHash($password);
        }
        if ($valid
            && ! (bool) env()['CHEVERETO_ENABLE_USERS']
            && getSetting('website_mode_personal_uid') != $userId
        ) {
            $valid = false;
        }
        if (! $valid) {
            $fail(401, 'invalid_credentials', 'Wrong username/email and password combination.');

            return;
        }
        $status = (string) ($user['status'] ?? '');
        $hasTwoFactor = TwoFactor::hasFor($userId);
        // The password is right. A two-step code that is being checked keeps the attempt row until it is verified, so
        // parallel code guesses are limited like password guesses; otherwise the attempt is not a failure.
        if ($status !== 'valid' || ! $hasTwoFactor || $code === '') {
            DB::delete('requests', ['id' => $attemptId]);
        }
        if ($accountError($status)) {
            return;
        }
        if ($hasTwoFactor) {
            if ($code === '') {
                $fail(401, 'two_factor_required', 'Two-factor authentication code required.');

                return;
            }
            $twoFactor = (new TwoFactor())->withSecret(TwoFactor::getSecretFor($userId));
            if (! $twoFactor->verify($code)) {
                DB::update('requests', ['type' => 'account-two-factor'], ['id' => $attemptId]);
                $fail(401, 'invalid_two_factor_code', 'Invalid two-factor authentication code.');

                return;
            }
            DB::delete('requests', ['id' => $attemptId]);
        }
        RequestLog::insert(['type' => 'login', 'user_id' => $userId, 'result' => 'success']);
        // Like a website login (Login::login), a successful sign-in clears this member's own failures from this IP.
        foreach ($failureTypes as $type) {
            RequestLog::delete(['user_id' => $userId, 'result' => 'fail', 'type' => $type, 'ip' => $ip]);
        }

        $name = $appKeyPrefix . ($device !== '' ? $device : 'Windows');
        // A computer signing in again replaces its previous key instead of piling up keys.
        DB::delete('api_keys', ['user_id' => $userId, 'name' => $name], 'AND');
        $key = ApiKey::insert($userId);
        $keyId = decodeID(explode('_', $key)[1]);
        DB::update('api_keys', ['name' => $name], ['id' => $keyId]);
        $appKeys = DB::fetchAllQuery(
            'SELECT api_key_id FROM `' . DB::getTable('api_keys') . '`'
            . ' WHERE api_key_user_id=:user_id AND api_key_name LIKE :prefix ORDER BY api_key_id DESC',
            [':user_id' => $userId, ':prefix' => $appKeyPrefix . '%']
        );
        foreach (array_slice($appKeys, $maxAppKeysPerUser) as $old) {
            ApiKey::remove((int) $old['api_key_id']);
        }
        $respond(200, [
            'api_key' => $key,
            'user' => $publicUser($user),
        ]);
    } catch (Throwable $e) {
        // Logged without the request data: Chevereto's own error handler would also log the posted password.
        error_log('upla-app ' . $action . ': ' . get_class($e) . ': ' . $e->getMessage() . ' in ' . $e->getFile() . ':' . $e->getLine());
        $fail(500, 'server_error', 'The server could not complete the request.');
    }
};

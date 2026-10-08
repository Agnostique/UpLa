<?php

/*
 * upla.com.tr: visits by bots do not count as video views.
 * A Chevereto 4.5.x route override: copy to <chevereto>/app/legacy/routes/overrides/video.php
 * (no core file is changed). Part of the UpLa desktop app, licensed under GPL v3; see Server/README.md.
 *
 * Chevereto's video route runs its core image route directly (not through image.php in this folder), so video pages
 * need their own wrapper. Videos are images to Chevereto and share the image_view_stock session list; see image.php for
 * how it works. image.php, video.php, album.php and tag.php in this folder use the same bot pattern; keep them in step.
 */

use Chevereto\Legacy\G\Handler;
use function Chevereto\Legacy\getIdFromURLComponent;
use function Chevereto\Legacy\getSetting;
use function Chevereto\Vars\server;
use function Chevereto\Vars\session;
use function Chevereto\Vars\sessionVar;

$coreRoute = require PATH_APP_LEGACY_ROUTES . 'video.php';

return function (Handler $handler) use ($coreRoute) {
    try {
        $agent = (string) (server()['HTTP_USER_AGENT'] ?? '');
        // "bot" must be followed by a separator, so phone names such as "CUBOT X30" are not taken for bots.
        $isBot = $agent === '' || preg_match(
            '~(?:bot|crawler)(?:[/;)_:-]|$)|spider|externalhit|externalagent|preview|headless|lighthouse|python-|curl/|wget/|go-http-client|okhttp|java/|httpclient|axios/|node-fetch|scrapy|postmanruntime|whatsapp|embedly|qwantify|mediapartners-google|adsbot-google|google-inspectiontool|storebot-google|googleother|bytespider|petalbot~i',
            $agent
        ) === 1;
        if ($isBot) {
            // Same request part as Chevereto's image route, which the video route runs.
            $request = getSetting('root_route') === 'image' ? $handler->requestArray() : $handler->request();
            $id = getIdFromURLComponent((string) ($request[0] ?? ''));
            $stock = session()['image_view_stock'] ?? [];
            if ($id > 0 && ! in_array($id, $stock)) {
                $stock[] = $id;
                sessionVar()->put('image_view_stock', $stock);
            }
        }
    } catch (Throwable) {
        // View bookkeeping must never break the page.
    }

    return $coreRoute($handler);
};

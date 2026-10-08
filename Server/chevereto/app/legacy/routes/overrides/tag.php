<?php

/*
 * upla.com.tr: visits by bots do not count as tag views.
 * A Chevereto 4.5.x route override: copy to <chevereto>/app/legacy/routes/overrides/tag.php
 * (no core file is changed). Part of the UpLa desktop app, licensed under GPL v3; see Server/README.md.
 *
 * Same as image.php, with Chevereto's tag_view_stock session list. A tag page can list several tags ("a,b"); all of them
 * are marked. image.php, video.php, album.php and tag.php in this folder use the same bot pattern; keep them in step.
 */

use Chevereto\Legacy\Classes\Tag;
use Chevereto\Legacy\G\Handler;
use function Chevereto\Vars\server;
use function Chevereto\Vars\session;
use function Chevereto\Vars\sessionVar;

$coreRoute = require PATH_APP_LEGACY_ROUTES . 'tag.php';

return function (Handler $handler) use ($coreRoute) {
    try {
        $agent = (string) (server()['HTTP_USER_AGENT'] ?? '');
        // "bot" must be followed by a separator, so phone names such as "CUBOT X30" are not taken for bots.
        $isBot = $agent === '' || preg_match(
            '~(?:bot|crawler)(?:[/;)_:-]|$)|spider|externalhit|externalagent|preview|headless|lighthouse|python-|curl/|wget/|go-http-client|okhttp|java/|httpclient|axios/|node-fetch|scrapy|postmanruntime|whatsapp|embedly|qwantify|mediapartners-google|adsbot-google|google-inspectiontool|storebot-google|googleother|bytespider|petalbot~i',
            $agent
        ) === 1;
        // Same tag key as Chevereto's tag route.
        $tagKey = rawurldecode((string) ($handler->request()[0] ?? ''));
        if ($isBot && $tagKey !== '') {
            $stock = session()['tag_view_stock'] ?? [];
            foreach (Tag::get($tagKey, 'id') as $tag) {
                if (isset($tag['id']) && ! in_array($tag['id'], $stock)) {
                    $stock[] = $tag['id'];
                }
            }
            sessionVar()->put('tag_view_stock', $stock);
        }
    } catch (Throwable) {
        // View bookkeeping must never break the page.
    }

    return $coreRoute($handler);
};

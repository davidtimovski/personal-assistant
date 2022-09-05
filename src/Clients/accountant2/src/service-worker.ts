import { build, files } from '$service-worker';

const APP_VERSION = '2.0.0-beta.3';

self.addEventListener('install', (event: ExtendableEvent) => {
	event.waitUntil(
		caches.open(APP_VERSION).then((cache) => {
			const preCacheResources = ['/'].concat(build).concat(files);
			cache.addAll(preCacheResources);
		})
	);
});

const channel = new BroadcastChannel('sw-version-updates');
self.addEventListener('activate', (event: ExtendableEvent) => {
	// Remove old caches
	event.waitUntil(
		caches
			.keys()
			.then((keyList) => {
				return Promise.all(
					keyList.map((key) => {
						if (key !== APP_VERSION) {
							return caches.delete(key);
						}
					})
				);
			})
			.then(() => {
				channel.postMessage({ version: APP_VERSION });
			})
	);
	return self.clients.claim();
});

self.addEventListener('fetch', (event: ExtendableEvent) => {
	if (event.request.url.includes('/api/')) {
		event.respondWith(fetch(event.request));
	} else {
		event.respondWith(
			caches.match(event.request).then((cachedResponse) => {
				return cachedResponse || fetch(event.request);
			})
		);
	}
});

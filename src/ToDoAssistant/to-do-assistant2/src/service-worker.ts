import { build, files } from '$service-worker';

const APP_VERSION = '2.3.7';

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

self.addEventListener('fetch', (event: FetchEvent) => {
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

// Push notification arrived
self.addEventListener('push', (event: PushEvent) => {
	const data = event.data?.json() ?? {};

	event.waitUntil(
		self.registration.showNotification(
			data.title,
			(options = {
				body: data.body,
				icon: data.senderImageUri,
				vibrate: [200, 100, 200],
				badge: data.senderImageUri,
				data: {
					openUrl: data.openUrl
				}
			})
		)
	);
});

// Push notification clicked
self.addEventListener('notificationclick', (event: NotificationEvent) => {
	const notification = event.notification;

	event.waitUntil(
		clients.matchAll().then((allClients) => {
			const client = allClients.find((c) => c.visibilityState === 'visible');

			if (client !== undefined) {
				client.navigate(notification.data.openUrl);
				client.focus();
			} else {
				clients.openWindow(notification.data.openUrl);
			}

			notification.close();
		})
	);
});

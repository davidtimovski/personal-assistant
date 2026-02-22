import { build, files } from '$service-worker';

const APP_VERSION = '2.4.3';
const ASSETS: string[] = [
	...build, // the app itself
	...files // everything in `static`
];

self.addEventListener('install', (event: ExtendableEvent) => {
	async function addFilesToCache() {
		const cache = await caches.open(APP_VERSION);
		await cache.addAll(ASSETS);
	}

	event.waitUntil(addFilesToCache());
});

const channel = new BroadcastChannel('sw-version-updates');
self.addEventListener('activate', (event: ExtendableEvent) => {
	async function deleteOldCaches() {
		const cacheKeys = await caches.keys();
		for (const key of cacheKeys) {
			if (key !== APP_VERSION) {
				await caches.delete(key);
			}
		}

		channel.postMessage({ version: APP_VERSION });
	}

	event.waitUntil(deleteOldCaches());
});

self.addEventListener('fetch', (event: FetchEvent) => {
	// Ignore non-GET and API calls
	if (event.request.method !== 'GET' || event.request.url.includes('/api/')) return;

	async function respond() {
		const url = new URL(event.request.url);
		const cache = await caches.open(APP_VERSION);

		// `build`/`files` can always be served from the cache
		if (ASSETS.includes(url.pathname)) {
			const response = await cache.match(url.pathname);
			if (response) {
				return response;
			}
		}

		// For all other non-API calls, try the network first, but
		// fall back to the cache if we're offline
		try {
			const response = await fetch(event.request);

			// If we're offline, fetch can return a value that is not a Response
			// instead of throwing - and we can't pass this non-Response to respondWith
			if (!(response instanceof Response)) {
				throw new Error('Invalid response from fetch');
			}

			if (response.status === 200) {
				cache.put(event.request, response.clone());
			}

			return response;
		} catch (err) {
			const response = await cache.match(event.request);

			if (response) {
				return response;
			}

			throw err;
		}
	}

	event.respondWith(respond());
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

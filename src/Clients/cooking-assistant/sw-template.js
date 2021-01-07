const CACHE_NAME = "1.9.21";
const precacheResources = [
  "#SHELL#",
  "#WEBFONTS#",
  "/manifest.json",
  "/robots.txt",
  "/favicon.png?v=2",
  "/images/icons/app-icon-48x48.png",
  "/images/icons/app-icon-96x96.png",
  "/images/icons/app-icon-144x144.png",
  "/images/icons/app-icon-192x192.png",
  "/images/icons/app-icon-256x256.png",
  "/images/icons/app-icon-384x384.png",
  "/images/icons/app-icon-512x512.png",
  "/",
  "/recipes",
];

self.addEventListener("install", (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      cache.addAll(precacheResources);
    })
  );
});

const channel = new BroadcastChannel("sw-version-updates");
self.addEventListener("activate", (event) => {
  // Remove old caches
  event.waitUntil(
    caches
      .keys()
      .then((keyList) => {
        return Promise.all(
          keyList.map((key) => {
            if (key !== CACHE_NAME) {
              return caches.delete(key);
            }
          })
        );
      })
      .then(() => {
        channel.postMessage({ version: CACHE_NAME });
      })
  );
  return self.clients.claim();
});

self.addEventListener("fetch", (event) => {
  if (event.request.url.includes("/api/")) {
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
self.addEventListener("push", (event) => {
  if (event.data) {
    var data = JSON.parse(event.data.text());
  }

  event.waitUntil(
    self.registration.showNotification(
      data.title,
      (options = {
        body: data.body,
        icon: data.senderImageUri,
        vibrate: [100, 50, 200],
        badge: data.senderImageUri,
        data: {
          openUrl: data.openUrl,
        },
      })
    )
  );
});

// Push notification clicked
self.addEventListener("notificationclick", (event) => {
  var notification = event.notification;

  event.waitUntil(
    clients.matchAll().then((allClients) => {
      if (notification.data.openUrl) {
        const client = allClients.find((c) => {
          return c.visibilityState === "visible";
        });

        if (client !== undefined) {
          client.navigate(notification.data.openUrl);
          client.focus();
        } else {
          clients.openWindow(notification.data.openUrl);
        }
      }

      notification.close();
    })
  );
});

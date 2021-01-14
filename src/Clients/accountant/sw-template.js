const CACHE_NAME = "1.13.0";
const precacheResources = [
  "#SHELL#",
  "#WEBFONTS#",
  "/manifest.json",
  "/robots.txt",
  "/favicon.png",
  "/images/icons/app-icon-48x48.png",
  "/images/icons/app-icon-96x96.png",
  "/images/icons/app-icon-144x144.png",
  "/images/icons/app-icon-192x192.png",
  "/images/icons/app-icon-256x256.png",
  "/images/icons/app-icon-384x384.png",
  "/images/icons/app-icon-512x512.png",
  "/",
  "/dashboard",
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

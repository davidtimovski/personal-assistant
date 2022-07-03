const CACHE_NAME = "1.20.1";
const precacheResources = [
  "#SHELL#",
  "#WEBFONTS#",
  "/manifest.json",
  "/robots.txt",
  "/favicon.png?v=3",
  "/images/icons/app-icon-x48.png",
  "/images/icons/app-icon-x48m.png",
  "/images/icons/app-icon-x72.png",
  "/images/icons/app-icon-x72m.png",
  "/images/icons/app-icon-x96.png",
  "/images/icons/app-icon-x96m.png",
  "/images/icons/app-icon-x128.png",
  "/images/icons/app-icon-x128m.png",
  "/images/icons/app-icon-x192.png",
  "/images/icons/app-icon-x192m.png",
  "/images/icons/app-icon-x384.png",
  "/images/icons/app-icon-x384m.png",
  "/images/icons/app-icon-x512.png",
  "/images/icons/app-icon-x512m.png",
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

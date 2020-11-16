const CACHE_NAME = "1.9.15";
const notificationIconUri = "/images/icons/app-icon-96x96.png";
const defaultProfileImageUri =
  "https://res.cloudinary.com/personalassistant/t_profile_thumbnail/production/defaults/sfmqac.jpg";
const precacheResources = [
  "/app~26dd54ad.d1507a238956917df82e.chunk.js",
  "/app~2fe2fb58.2c417794a1001d0c795c.chunk.js",
  "/app~493df0b3.3a3fe51013be4f3ceb8b.chunk.js",
  "/app~616d6534.dd2de6c593d6a581a2b7.chunk.js",
  "/app~7d0fd2eb.cee3e1de67072d1dac4b.chunk.js",
  "/app~9372e760.14d167e2c9dc5b6f53f8.chunk.js",
  "/app~99490b19.80eec0c11db898719653.chunk.js",
  "/app~a6aa1f09.618b26c5e41b1ef03aae.chunk.js",
  "/app~afa9144c.62567f02bcf0e1089455.chunk.js",
  "/app~b69a0d34.80c782750e9f9f4ef7ba.chunk.js",
  "/app~c714bc7b.f84c850eab14ca1d43e6.chunk.js",
  "/app~d9fca19d.be08b0cbd7937edef0e1.chunk.js",
  "/app~ea1f58e8.1947cf17e11ca32d0cee.chunk.js",
  "/app~f71cff67.f4f880d781a03e00932d.chunk.js",
  "/en-US~f7c7cc65.e2ea2e7c1f6837b10647.chunk.js",
  "/mk-MK~5953f7e0.bb3556f0a0bef681954c.chunk.js",
  "/runtime~app.24d04d9d411fea4d79fe.bundle.js",
  "/vendor.aurelia-animator-css~be6b90b8.0dea6a128a30df4b581b.chunk.js",
  "/vendor.aurelia-binding~4157a4ee.e01198afb4fc1a2bf161.chunk.js",
  "/vendor.aurelia-dependency-injection~fbc292be.1656b1ba8b1025ca683e.chunk.js",
  "/vendor.aurelia-fetch-client~e761a4f7.b9cf85da894002f5e204.chunk.js",
  "/vendor.aurelia-framework~b33c7998.0bc6ea0e6583426906e1.chunk.js",
  "/vendor.aurelia-history-browser~dfbf2ea6.e43319d51e4433f8c331.chunk.js",
  "/vendor.aurelia-i18n~e76532a0.dbacbdd6a882ef43cbd3.chunk.js",
  "/vendor.aurelia-loader-webpack~6926fbb5.b7e63ac413f5ed7a0b97.chunk.js",
  "/vendor.aurelia-pal-browser~d6c0e73b.bff061470c8af713658b.chunk.js",
  "/vendor.aurelia-polyfills~dc8a991d.8cbe8bce426edd1ce4d3.chunk.js",
  "/vendor.aurelia-route-recognizer~8134d493.d5ec1878600a375da3fb.chunk.js",
  "/vendor.aurelia-router~ae38da23.f8880e46fac32a4962c3.chunk.js",
  "/vendor.aurelia-store~27da0bf4.0dcdce9b3d49a63fca77.chunk.js",
  "/vendor.aurelia-templating-binding~6483373c.d417e9d75a95986b01b8.chunk.js",
  "/vendor.aurelia-templating-resources~2fe83516.60334dccb54bc1685770.chunk.js",
  "/vendor.aurelia-templating-router~be592036.5db579463dc1cefe1da9.chunk.js",
  "/vendor.aurelia-templating~9e0f4621.3768a04f574525653897.chunk.js",
  "/vendor.aurelia-validation~6903c527.26174baf916fbb9dc48a.chunk.js",
  "/vendor.autocompleter~5c5992e7.472a21953092feece016.chunk.js",
  "/vendor.i18next~664b5b6d.4f953bce4736eba641f2.chunk.js",
  "/vendor.i18next~b575809c.8433c48934e5f5f0e5ee.chunk.js",
  "/vendor.i18next~b8b88f58.fb32b3a5fc7e23499e6a.chunk.js",
  "/vendor.oidc-client~640aaa4b.a693e409353d554a4613.chunk.js",
  "/vendor.rxjs~6fdc9279.5fb1635198a888e866aa.chunk.js",
  "/vendor.rxjs~789afed8.9e12bb8115d58c0df22f.chunk.js",
  "/vendor.tslib~73d8ab38.581dd2ec9942f451d4b0.chunk.js",
  "/vendors~2a42e354.b9aa51d5218fa51ddaa7.chunk.js",
  "/vendors~4c12d43a.64071f473979b0c41ee5.chunk.js",
  "https://fonts.googleapis.com/css?family=Didact+Gothic&display=swap",
  "/manifest.json",
  "/robots.txt",
  "/favicon.png?v=2",
  defaultProfileImageUri,
  "/images/icons/app-icon-48x48.png",
  notificationIconUri,
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

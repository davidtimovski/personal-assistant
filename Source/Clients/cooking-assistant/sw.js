const CACHE_NAME = "1.9.11";
const notificationIconUri = "/images/icons/app-icon-96x96.png";
const defaultProfileImageUri =
  "https://res.cloudinary.com/personalassistant/t_profile_thumbnail/production/defaults/sfmqac.jpg";
const precacheResources = [
  "/app~26dd54ad.077f90f380ae8c188e7b.chunk.js",
  "/app~2fe2fb58.cd0b636312c0eae55d08.chunk.js",
  "/app~493df0b3.182be156e0f483a019d5.chunk.js",
  "/app~616d6534.b4d10d63b79242b3e40e.chunk.js",
  "/app~7d0fd2eb.5cf17f5c090035540bad.chunk.js",
  "/app~9372e760.d8757ec91d4a96a1fc60.chunk.js",
  "/app~99490b19.55fb405705e0dd3ab7cb.chunk.js",
  "/app~a6aa1f09.79a5115e7ef8099329e6.chunk.js",
  "/app~afa9144c.92920f9ae67810d6da43.chunk.js",
  "/app~b69a0d34.0610d1ee761d1c15dab4.chunk.js",
  "/app~c714bc7b.45ef7ddf29052c215e3c.chunk.js",
  "/app~d9fca19d.afcf5dff506f7b37270e.chunk.js",
  "/app~ea1f58e8.eb6a9f5b01dd07c76fa7.chunk.js",
  "/app~f71cff67.72de6c1d0e4437f49553.chunk.js",
  "/en-US~f7c7cc65.d6872fbd5dd3cd62d41d.chunk.js",
  "/mk-MK~5953f7e0.79cc3abf45dfb96a5d1d.chunk.js",
  "/runtime~app.9be1987b087302367c3f.bundle.js",
  "/vendor.aurelia-animator-css~be6b90b8.484884b14607036a4df4.chunk.js",
  "/vendor.aurelia-binding~4157a4ee.a967ec0c8b14207e387f.chunk.js",
  "/vendor.aurelia-dependency-injection~fbc292be.8ee0ddd190bd8340ec47.chunk.js",
  "/vendor.aurelia-fetch-client~e761a4f7.02df5e9fd877109c7667.chunk.js",
  "/vendor.aurelia-framework~b33c7998.7ee3887e04c04a36aee9.chunk.js",
  "/vendor.aurelia-history-browser~dfbf2ea6.5a1c808cf9e8d2a21518.chunk.js",
  "/vendor.aurelia-i18n~e76532a0.b5d5c4e9611200b89014.chunk.js",
  "/vendor.aurelia-loader-webpack~6926fbb5.7c145aa66e02317506e5.chunk.js",
  "/vendor.aurelia-pal-browser~d6c0e73b.b3fbb587d61cbb50f143.chunk.js",
  "/vendor.aurelia-polyfills~dc8a991d.d7d74f52691b4e4447c1.chunk.js",
  "/vendor.aurelia-route-recognizer~8134d493.1deb3fc6bcda0d661cb2.chunk.js",
  "/vendor.aurelia-router~ae38da23.b78924d37817e4170a0d.chunk.js",
  "/vendor.aurelia-store~27da0bf4.008cfee6803912042f64.chunk.js",
  "/vendor.aurelia-templating-binding~6483373c.53ed4dbd7ba27f85895b.chunk.js",
  "/vendor.aurelia-templating-resources~2fe83516.a7fd1d5dcb21fbb3a919.chunk.js",
  "/vendor.aurelia-templating-router~be592036.3ab0c1320e6592342ca4.chunk.js",
  "/vendor.aurelia-templating~9e0f4621.e0d81947d56c324c7872.chunk.js",
  "/vendor.aurelia-validation~6903c527.9bee509a4b7a545be66c.chunk.js",
  "/vendor.autocompleter~5c5992e7.b732667673c0d22e5657.chunk.js",
  "/vendor.i18next~664b5b6d.13840bb4d113e6eedcd8.chunk.js",
  "/vendor.i18next~b575809c.37be998cf79401e1a2ed.chunk.js",
  "/vendor.i18next~b8b88f58.ac515a99975daf93d38c.chunk.js",
  "/vendor.oidc-client~640aaa4b.6e84dc81821304f970c6.chunk.js",
  "/vendor.rxjs~6fdc9279.b51de6e907007064428e.chunk.js",
  "/vendor.rxjs~789afed8.932c6cf3f1a1609ab30b.chunk.js",
  "/vendors~2a42e354.8212eea93f4fca774f13.chunk.js",
  "/vendors~4c12d43a.3e1e0537255104fe8475.chunk.js",
  "/vendors~f9ca8911.9429c78c66442559429d.chunk.js",
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

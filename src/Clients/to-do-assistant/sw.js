const CACHE_NAME = "1.8.3";
const notificationIconUri = "/images/icons/app-icon-96x96.png";
const defaultProfileImageUri =
  "https://res.cloudinary.com/personalassistant/t_profile_thumbnail/production/defaults/sfmqac.jpg";
const precacheResources = [
  "/app~09a9cf39.1ad0f2f5539e11d03787.chunk.js",
  "/app~26dd54ad.414a8f96a4833f91daaa.chunk.js",
  "/app~2fe2fb58.984a5e3f61983fde3922.chunk.js",
  "/app~493df0b3.3887d66e42382def9c91.chunk.js",
  "/app~616d6534.b06273db37bca9a669e0.chunk.js",
  "/app~62fef08f.da95a90fd88ec141605a.chunk.js",
  "/app~7d0fd2eb.6f301a2d7716a8322b38.chunk.js",
  "/app~ce58d926.543d779cc0c02d807f0c.chunk.js",
  "/app~ea1f58e8.8a2ab51815a8fc8641f2.chunk.js",
  "/app~f4de321d.e06e8dfd5f0ce97674d2.chunk.js",
  "/app~f71cff67.352d2a53a957fe04a00a.chunk.js",
  "/app~f7ca70b6.3226e64dd6681a664179.chunk.js",
  "/en-US~f7c7cc65.2c57aa6c169343bff983.chunk.js",
  "/mk-MK~5953f7e0.931d0d335d0e60ee658d.chunk.js",
  "/runtime~app.3318c0647fdeb5d79b07.bundle.js",
  "/vendor.aurelia-animator-css~be6b90b8.618c01a309858044a08a.chunk.js",
  "/vendor.aurelia-binding~4157a4ee.b5c62c1ec52f1780061f.chunk.js",
  "/vendor.aurelia-dependency-injection~fbc292be.85d01715640ea11c8dab.chunk.js",
  "/vendor.aurelia-fetch-client~e761a4f7.b0af1605b54ef1ac6618.chunk.js",
  "/vendor.aurelia-framework~b33c7998.aa657167cf581c8a2847.chunk.js",
  "/vendor.aurelia-history-browser~dfbf2ea6.2ad78e3248ca60cfd7ad.chunk.js",
  "/vendor.aurelia-i18n~e76532a0.d0b5605bfc0987755417.chunk.js",
  "/vendor.aurelia-loader-webpack~6926fbb5.c3a3b6ac8055285e4174.chunk.js",
  "/vendor.aurelia-pal-browser~d6c0e73b.bf730f4dfdded6daddcd.chunk.js",
  "/vendor.aurelia-polyfills~dc8a991d.8c70dead3b113e618b23.chunk.js",
  "/vendor.aurelia-route-recognizer~8134d493.fa280072c406a18e11c1.chunk.js",
  "/vendor.aurelia-router~ae38da23.7aad51319d3149cd5688.chunk.js",
  "/vendor.aurelia-store~27da0bf4.ae0b6d14d8c6eba4f264.chunk.js",
  "/vendor.aurelia-templating-binding~6483373c.60291ef0456034ab136f.chunk.js",
  "/vendor.aurelia-templating-resources~2fe83516.83ebc54e3f227e5382a7.chunk.js",
  "/vendor.aurelia-templating-router~be592036.5ab1f6c2fd8e0094544f.chunk.js",
  "/vendor.aurelia-templating~9e0f4621.4d828276c3ad9c9091e9.chunk.js",
  "/vendor.aurelia-validation~6903c527.e2c44aa456439b6a3111.chunk.js",
  "/vendor.bcx-aurelia-dnd~feda4413.4733e30e865ab72c924a.chunk.js",
  "/vendor.bcx-aurelia-reorderable-repeat~c1357074.fb2f8d1108ff4a84d2c1.chunk.js",
  "/vendor.i18next~664b5b6d.6add3df209a1f3b213b5.chunk.js",
  "/vendor.i18next~b575809c.84a116edc9377fe131d6.chunk.js",
  "/vendor.i18next~b8b88f58.7da9e87593cf606e7067.chunk.js",
  "/vendor.oidc-client~640aaa4b.d5ea7868dbc856a64b8b.chunk.js",
  "/vendor.rxjs~6fdc9279.e17ca82c994b8af3d704.chunk.js",
  "/vendor.rxjs~789afed8.7f9cecb6acafab189e35.chunk.js",
  "/vendor.tslib~73d8ab38.6fe2c7000cd139f8c0e8.chunk.js",
  "/vendors~2a42e354.9bca832416970a9c52d5.chunk.js",
  "/vendors~4c12d43a.dcd34670c3280e0dad6a.chunk.js",
  "https://fonts.googleapis.com/css?family=Didact+Gothic&display=swap",
  "/manifest.json",
  "/robots.txt",
  "/favicon.png?v=7",
  defaultProfileImageUri,
  "/images/icons/app-icon-48x48.png",
  notificationIconUri,
  "/images/icons/app-icon-144x144.png",
  "/images/icons/app-icon-192x192.png",
  "/images/icons/app-icon-256x256.png",
  "/images/icons/app-icon-384x384.png",
  "/images/icons/app-icon-512x512.png",
  "/audio/bleep.mp3",
  "/audio/blop.mp3",
  "/",
  "/lists",
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
      const client = allClients.find((c) => {
        return c.visibilityState === "visible";
      });

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

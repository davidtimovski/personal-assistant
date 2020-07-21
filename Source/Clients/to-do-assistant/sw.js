const CACHE_NAME = "1.7.7";
const notificationIconUri = "/images/icons/app-icon-96x96.png";
const defaultProfileImageUri =
  "https://res.cloudinary.com/personalassistant/t_profile_thumbnail/production/defaults/sfmqac.jpg";
const precacheResources = [
  "/app~09a9cf39.25c43cbf63d7a793ede4.chunk.js",
  "/app~26dd54ad.591e26dd566374377a6f.chunk.js",
  "/app~2fe2fb58.0e4bd83e244122ef3ca7.chunk.js",
  "/app~493df0b3.33632d88cdeae9cafdbe.chunk.js",
  "/app~616d6534.0c36577d54fc19221c78.chunk.js",
  "/app~62fef08f.fe07a35106fc90e54cdf.chunk.js",
  "/app~7d0fd2eb.90169f55b2130a1b9d80.chunk.js",
  "/app~b98765b7.7ff574490ec18b109c5f.chunk.js",
  "/app~ce58d926.e1965f502854a8c5e2d0.chunk.js",
  "/app~ea1f58e8.e6fb8d54d825ae9d7080.chunk.js",
  "/app~f4de321d.46c170d6e84126528d04.chunk.js",
  "/app~f71cff67.609a36177c4b223eb269.chunk.js",
  "/en-US~f7c7cc65.4860dd5f1d5159c780f9.chunk.js",
  "/mk-MK~5953f7e0.5d8b07796a22763c0160.chunk.js",
  "/runtime~app.a44846fa22a6483e9486.bundle.js",
  "/vendor.aurelia-animator-css~be6b90b8.42aa524110b7a76a1405.chunk.js",
  "/vendor.aurelia-binding~4157a4ee.13463ccd3acc69d72324.chunk.js",
  "/vendor.aurelia-dependency-injection~fbc292be.3b1b8b416aff1f908f29.chunk.js",
  "/vendor.aurelia-fetch-client~e761a4f7.15ebad1e252ce616699f.chunk.js",
  "/vendor.aurelia-framework~b33c7998.3378359c2ff8bbef611d.chunk.js",
  "/vendor.aurelia-history-browser~dfbf2ea6.5a7b69f12be272605728.chunk.js",
  "/vendor.aurelia-i18n~e76532a0.d76ceea226203a59bb83.chunk.js",
  "/vendor.aurelia-loader-webpack~6926fbb5.d22109bb2ebd505f7963.chunk.js",
  "/vendor.aurelia-pal-browser~d6c0e73b.12e664f8aa5958a2246b.chunk.js",
  "/vendor.aurelia-polyfills~dc8a991d.25ee21f52b3c9aaa365a.chunk.js",
  "/vendor.aurelia-route-recognizer~8134d493.66bded25b9ddbe5fe264.chunk.js",
  "/vendor.aurelia-router~ae38da23.8a09af72c712afd3b49f.chunk.js",
  "/vendor.aurelia-templating-binding~6483373c.1652909f2313c8588149.chunk.js",
  "/vendor.aurelia-templating-resources~2fe83516.9d4265f6bb9726a53c53.chunk.js",
  "/vendor.aurelia-templating-router~be592036.3ec77778b36b02f6d2f2.chunk.js",
  "/vendor.aurelia-templating~9e0f4621.ff8e005260cef61cfa3d.chunk.js",
  "/vendor.aurelia-validation~6903c527.c892d1334fcb1d822c4c.chunk.js",
  "/vendor.bcx-aurelia-dnd~eda42ed5.5bf38542768fba7b25fb.chunk.js",
  "/vendor.bcx-aurelia-reorderable-repeat~c1357074.03d113ca88acebda86d2.chunk.js",
  "/vendor.dexie~dff86cf2.384482bb3ed11898e925.chunk.js",
  "/vendor.i18next~664b5b6d.b225c898f44c5a55bd2c.chunk.js",
  "/vendor.i18next~b575809c.112b032cdc21d9e73e65.chunk.js",
  "/vendor.i18next~b8b88f58.990adfdff70b0c357019.chunk.js",
  "/vendor.oidc-client~640aaa4b.40823044a60c303c5922.chunk.js",
  "/vendors~2a42e354.98af7b500e42d685d43f.chunk.js",
  "/vendors~4c12d43a.d9f2c7c01386dc309384.chunk.js",
  "/vendors~f9ca8911.229ebcc82af81923a79b.chunk.js",
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

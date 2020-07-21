const CACHE_NAME = "1.10.11";
const notificationIconSrc = "/images/icons/app-icon-96x96.png";
const defaultProfileImageUri =
  "https://res.cloudinary.com/personalassistant/t_profile_thumbnail/production/defaults/sfmqac.jpg";
const precacheResources = [
  "/app~0837cc83.916da4efd3fd27bf9a4a.chunk.js",
  "/app~26dd54ad.cfc9e66ce98cf0dbf52e.chunk.js",
  "/app~37306aed.79d58e561b56e09e9789.chunk.js",
  "/app~3e511cab.4ded482db81afff357e5.chunk.js",
  "/app~4290e0f1.00bcd2890266d737f1ef.chunk.js",
  "/app~493df0b3.8670ce4fe663ae453d71.chunk.js",
  "/app~4c7ba33a.d2383f363a2b6545b015.chunk.js",
  "/app~616d6534.70eadc000ed5d615c9df.chunk.js",
  "/app~7d0fd2eb.dd785e7a1a26dcb62e08.chunk.js",
  "/app~a6aa1f09.4a76d75f805302134eb9.chunk.js",
  "/app~b8df104a.c7d22cddbd22b77c16b7.chunk.js",
  "/app~b98765b7.7a223c6724916f30534c.chunk.js",
  "/app~c26642c1.87549be9184aabc06be2.chunk.js",
  "/app~c4aa8f83.4aabc4b7ca411487b2ad.chunk.js",
  "/app~c714bc7b.1b9378a03a271927601c.chunk.js",
  "/app~d2f94771.c38d46d74d024ae4be1a.chunk.js",
  "/app~ddfc7f97.233a72b2f8a2cfe5486f.chunk.js",
  "/app~f71cff67.6347de4f673b74c99e38.chunk.js",
  "/en-US~f7c7cc65.225a154662ca280591b2.chunk.js",
  "/mk-MK~5953f7e0.44daa2f62004918d1bf3.chunk.js",
  "/runtime~app.d9a516d0695bcf304f76.bundle.js",
  "/vendor.aurelia-animator-css~be6b90b8.078381e647fc5ecde756.chunk.js",
  "/vendor.aurelia-binding~4157a4ee.6937682858bc25066e26.chunk.js",
  "/vendor.aurelia-dependency-injection~fbc292be.a729e2fa80ea01751f76.chunk.js",
  "/vendor.aurelia-fetch-client~e761a4f7.fbfd9d6c89bd9b82b1f3.chunk.js",
  "/vendor.aurelia-framework~b33c7998.604b898cfcae5e0418fb.chunk.js",
  "/vendor.aurelia-history-browser~dfbf2ea6.fa949c6041cabd75e1f1.chunk.js",
  "/vendor.aurelia-i18n~e76532a0.022d393732fdca88ecf3.chunk.js",
  "/vendor.aurelia-loader-webpack~6926fbb5.a861562ec368e1f437ab.chunk.js",
  "/vendor.aurelia-pal-browser~d6c0e73b.974afe91739a16ba0477.chunk.js",
  "/vendor.aurelia-polyfills~dc8a991d.2104edcdae7a090d0253.chunk.js",
  "/vendor.aurelia-route-recognizer~8134d493.914823ff38319ca55bee.chunk.js",
  "/vendor.aurelia-router~ae38da23.93fd921a2913bafc012a.chunk.js",
  "/vendor.aurelia-store~27da0bf4.0aa1f09b11cd9fc5bac3.chunk.js",
  "/vendor.aurelia-templating-binding~6483373c.081dd6cb1ea488b265bf.chunk.js",
  "/vendor.aurelia-templating-resources~2fe83516.1b180c43729f3e23c911.chunk.js",
  "/vendor.aurelia-templating-router~be592036.fe97f100db2a6fd525a0.chunk.js",
  "/vendor.aurelia-templating~9e0f4621.010d98406222cd85b28b.chunk.js",
  "/vendor.aurelia-validation~6903c527.97160e7f8176c0f22421.chunk.js",
  "/vendor.autocompleter~5c5992e7.3c514cd03cbc1d0f45d9.chunk.js",
  "/vendor.chart.js~46fbf940.beed0f8700094047615a.chunk.js",
  "/vendor.dexie~dff86cf2.bba7f46efd8ad015cb8d.chunk.js",
  "/vendor.i18next~664b5b6d.bd1287e9ef53bf0b96ea.chunk.js",
  "/vendor.i18next~b575809c.c22e4940bb32df3c3dde.chunk.js",
  "/vendor.i18next~b8b88f58.79335e86f26a0fa33688.chunk.js",
  "/vendor.moment~3dbf943e.762fc6ba99b7d92475c5.chunk.js",
  "/vendor.moment~43faca05.f54845275477db5e8fba.chunk.js",
  "/vendor.moment~514c2335.1b1a597f08c6e028190b.chunk.js",
  "/vendor.moment~5858bbea.9754a44bb4de4fb18fa1.chunk.js",
  "/vendor.moment~b2fb4db9.bbe62017941c2509f305.chunk.js",
  "/vendor.moment~bace3e66.156d8e063d15d1926086.chunk.js",
  "/vendor.moment~dfd12c22.db141b20b6bda7b14f9f.chunk.js",
  "/vendor.moment~e3be9bc9.a258ee64de371ce7efb2.chunk.js",
  "/vendor.oidc-client~640aaa4b.cfd26d7e80a7e98e05b3.chunk.js",
  "/vendor.rxjs~6fdc9279.d6c1104cfd73fe934429.chunk.js",
  "/vendor.rxjs~789afed8.adeb28b44dca092af24f.chunk.js",
  "/vendors~2a42e354.2977107530670845435f.chunk.js",
  "/vendors~4c12d43a.8adab30830f9d4675e51.chunk.js",
  "/vendors~ec8c427e.7770fddf7995be15496b.chunk.js",
  "https://fonts.googleapis.com/css?family=Didact+Gothic&display=swap",
  "/manifest.json",
  "/robots.txt",
  "/favicon.png",
  defaultProfileImageUri,
  "/images/icons/app-icon-48x48.png",
  notificationIconSrc,
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

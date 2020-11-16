const CACHE_NAME = "1.11.3";
const notificationIconSrc = "/images/icons/app-icon-96x96.png";
const defaultProfileImageUri =
  "https://res.cloudinary.com/personalassistant/t_profile_thumbnail/production/defaults/sfmqac.jpg";
const precacheResources = [
  "/app~0837cc83.f9b15a8f175260d4a684.chunk.js",
  "/app~26dd54ad.059abab2f272e1722e28.chunk.js",
  "/app~37306aed.5f961a9fe7bebf72617d.chunk.js",
  "/app~3e511cab.80cbcaaeab87e64261a3.chunk.js",
  "/app~4290e0f1.7c4033967903df90128e.chunk.js",
  "/app~493df0b3.437555a323a5cd1ae499.chunk.js",
  "/app~4c7ba33a.f5e29d511c2ee264b8c3.chunk.js",
  "/app~616d6534.47ae70217b0ba13a31fe.chunk.js",
  "/app~7d0fd2eb.9c61ab48b9ab06e19a78.chunk.js",
  "/app~8b9f7e93.e51fd867f30ae8425f11.chunk.js",
  "/app~a6aa1f09.2dbd244d524d9ad42205.chunk.js",
  "/app~b8df104a.f804cade1afa592b2da9.chunk.js",
  "/app~b98765b7.e0c366dbb7e619c8b321.chunk.js",
  "/app~c26642c1.6396030ceb706b385349.chunk.js",
  "/app~c4aa8f83.e4cfc266fea03bbf73dd.chunk.js",
  "/app~c714bc7b.ce8c6f2c7dc3789d0be0.chunk.js",
  "/app~d2f94771.6610215cf10251d7a709.chunk.js",
  "/app~ddfc7f97.34b631ad15839cf73be1.chunk.js",
  "/app~e2550e02.2c77fafc1de6a44e1d56.chunk.js",
  "/en-US~f7c7cc65.3eee0fff5f8cd0d3ae1b.chunk.js",
  "/mk-MK~5953f7e0.42fa10c49b0ea55890be.chunk.js",
  "/runtime~app.ef6d645da7234fe23acf.bundle.js",
  "/vendor.aurelia-animator-css~be6b90b8.176aa02bb86e980c323e.chunk.js",
  "/vendor.aurelia-binding~4157a4ee.7441b48ea9b068ccc8a7.chunk.js",
  "/vendor.aurelia-dependency-injection~fbc292be.2731804303adb55f1c7f.chunk.js",
  "/vendor.aurelia-fetch-client~e761a4f7.a2ece58a42c67f46374e.chunk.js",
  "/vendor.aurelia-framework~b33c7998.dba1c3c26872f5cd59a3.chunk.js",
  "/vendor.aurelia-history-browser~dfbf2ea6.09adc95c0af7965c97bd.chunk.js",
  "/vendor.aurelia-i18n~e76532a0.a9f8b465b6118e4536b8.chunk.js",
  "/vendor.aurelia-loader-webpack~6926fbb5.883b0af676f364b83ef6.chunk.js",
  "/vendor.aurelia-pal-browser~d6c0e73b.8dd4072f09da2e6ec1da.chunk.js",
  "/vendor.aurelia-polyfills~dc8a991d.a4f39465be2ed04cd0b6.chunk.js",
  "/vendor.aurelia-route-recognizer~8134d493.8015d6fa4a07675ae350.chunk.js",
  "/vendor.aurelia-router~ae38da23.e07c4f9104460706f183.chunk.js",
  "/vendor.aurelia-store~27da0bf4.0ed65e4d9fabfe1edfb6.chunk.js",
  "/vendor.aurelia-templating-binding~6483373c.e29918e9397564614d56.chunk.js",
  "/vendor.aurelia-templating-resources~2fe83516.b20c75c2423180423824.chunk.js",
  "/vendor.aurelia-templating-router~be592036.b93711eb0bd3000861b6.chunk.js",
  "/vendor.aurelia-templating~9e0f4621.140074c085a2d6866c2a.chunk.js",
  "/vendor.aurelia-validation~6903c527.364f695b2494203eace8.chunk.js",
  "/vendor.autocompleter~5c5992e7.e460fe820556ff6c2912.chunk.js",
  "/vendor.chart.js~46fbf940.e1d6a7aea4a1b2213b7c.chunk.js",
  "/vendor.dexie~dff86cf2.9a955a371eb713400eca.chunk.js",
  "/vendor.i18next~664b5b6d.0f37c95a6b512f20b90c.chunk.js",
  "/vendor.i18next~b575809c.fbc647dc6213d539a15a.chunk.js",
  "/vendor.i18next~b8b88f58.6058b045ddcb6c651b88.chunk.js",
  "/vendor.moment~0a56fd24.20b5a85b4534db6a51c0.chunk.js",
  "/vendor.moment~13aac730.c243cae89dfa348a34d9.chunk.js",
  "/vendor.moment~15f0789d.c3c1da86c8cb1d562172.chunk.js",
  "/vendor.moment~1702d7fa.d0843a339baad5c1bc47.chunk.js",
  "/vendor.moment~1d8d93f8.d1d6f7375f3b4d10985f.chunk.js",
  "/vendor.moment~1e23895a.b4cf5faab69db3d2334e.chunk.js",
  "/vendor.moment~31c708a5.f8b67138ec593db261b6.chunk.js",
  "/vendor.moment~3867e4ef.8a857a5da86a9bad8b77.chunk.js",
  "/vendor.moment~399b027d.2164b522150e631adaf9.chunk.js",
  "/vendor.moment~5f43c043.298203916a35fc598397.chunk.js",
  "/vendor.moment~679e2cd8.fac42775165698033c0f.chunk.js",
  "/vendor.moment~7d909a97.ad6035c6687218c0910d.chunk.js",
  "/vendor.moment~7db804d5.77f43e869c82882013c4.chunk.js",
  "/vendor.moment~8a7b4606.5fe032d660100e02d3b2.chunk.js",
  "/vendor.moment~8c8f66f3.521b62c582d558236a22.chunk.js",
  "/vendor.moment~8eeb4602.8254caa2c4c8d71c7f3c.chunk.js",
  "/vendor.moment~8f0f48a5.6dd9892c00f6becc3f01.chunk.js",
  "/vendor.moment~9f874da7.10ae90a1082c0500b215.chunk.js",
  "/vendor.moment~a884eab2.857888c878a5cb658f2f.chunk.js",
  "/vendor.moment~b5b59692.1f7b33fde98a6e91cbc7.chunk.js",
  "/vendor.moment~b895b8ba.2ac1026d6dd59345512a.chunk.js",
  "/vendor.moment~bc6e0817.a1901039614e8fcf6481.chunk.js",
  "/vendor.moment~cc99a214.61e10acebf0e8dc621d9.chunk.js",
  "/vendor.moment~d22b72d1.51d581a8f4a927075501.chunk.js",
  "/vendor.moment~e258e298.50ce8542b37ca99e67fc.chunk.js",
  "/vendor.moment~e42b2e82.e1002ae9bf453120e0c0.chunk.js",
  "/vendor.moment~eb9f9df4.cb72eec30a2cf7223a8b.chunk.js",
  "/vendor.oidc-client~640aaa4b.69e0add6258be3fbbeee.chunk.js",
  "/vendor.rxjs~6fdc9279.11d09da0b1bbc757595f.chunk.js",
  "/vendor.rxjs~789afed8.8d5c892fe7c3c385da16.chunk.js",
  "/vendor.tslib~73d8ab38.2dcce2532fcd33dc93a4.chunk.js",
  "/vendors~2a42e354.7ed8e915668ca40a258e.chunk.js",
  "/vendors~4c12d43a.45640fe33b2fcf57e90c.chunk.js",
  "/vendors~ec8c427e.bb52b17e05b47f3a003f.chunk.js",
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

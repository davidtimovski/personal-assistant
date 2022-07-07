import { autoinject } from "aurelia-framework";

import { HttpProxy } from "../utils/httpProxy";
import { LocalStorageBase } from "../utils/localStorageBase";
import { ErrorLogger } from "./errorLogger";

@autoinject
export class UsersServiceBase {
  private readonly profileImageThumbRegex =
    /res.cloudinary.com\/personalassistant\/t_profile_thumbnail\/(development|production)\/users/g;

  constructor(
    protected readonly httpProxy: HttpProxy,
    protected readonly logger: ErrorLogger,
    private readonly localStorageBase: LocalStorageBase
  ) {}

  async getProfileImageUri(): Promise<string> {
    try {
      const currentSrc = this.localStorageBase.getProfileImageUri();

      if (!navigator.onLine) {
        return currentSrc;
      }

      const src = await this.httpProxy.ajax<string>("api/users/profile-image-uri");
      if (!src) {
        throw new Error("Could not load profile image URI");
      }

      this.localStorageBase.setProfileImageUriLastLoad(new Date());

      if (src !== currentSrc) {
        this.localStorageBase.setProfileImageUri(src);

        // Remove previous profile images from cache and add new one
        caches.keys().then(async (cacheNames) => {
          if (cacheNames.length > 0) {
            const latest = cacheNames.sort().reverse()[0];
            const cacheStorage = await caches.open(latest);

            for (const request of await cacheStorage.keys()) {
              if (request.url.match(this.profileImageThumbRegex)) {
                cacheStorage.delete(request);
              }
            }

            await cacheStorage.add(new Request(src));
          }
        });
      }

      return src;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }
}

import { HttpProxy } from "../services/httpProxy";
import { LocalStorageBase } from "../utils/localStorageBase";
import { ErrorLogger } from "./errorLogger";
import Variables from "$lib/variables";

export class UsersServiceBase {
  private readonly profileImageThumbRegex =
    /res.cloudinary.com\/personalassistant\/t_profile_thumbnail\/(development|production)\/users/g;

  protected readonly httpProxy = new HttpProxy();
  protected readonly localStorageBase = new LocalStorageBase();
  protected readonly logger: ErrorLogger;

  constructor(application: string) {
    this.logger = new ErrorLogger(application);
  }

  profileImageUriIsStale(): boolean {
    const lastLoadValue = this.localStorageBase.get("profileImageUriLastLoad");
    const lastLoad = lastLoadValue ? parseInt(lastLoadValue, 10) : 0;

    const diff = new Date().getTime() - lastLoad;
    const minutes = Math.floor(diff / 1000 / 60);

    return minutes > 1440;
  }

  async getProfileImageUri(): Promise<string> {
    try {
      const currentSrc = this.localStorageBase.get("profileImageUri");

      if (!navigator.onLine) {
        return currentSrc;
      }

      const src = await this.httpProxy.ajax<string>(
        `${Variables.urls.api}/api/users/profile-image-uri`
      );
      if (!src) {
        throw new Error("Could not load profile image URI");
      }

      this.localStorageBase.set(
        "profileImageUriLastLoad",
        new Date().getTime().toString()
      );

      if (src !== currentSrc) {
        this.localStorageBase.set("profileImageUri", src);

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

  release() {
    this.httpProxy.release();
    this.logger.release();
  }
}

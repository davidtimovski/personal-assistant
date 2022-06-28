import { inject } from "aurelia-framework";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";

import { AuthService } from "./authService";
import { LocalStorageBase } from "../utils/localStorageBase";

@inject(AuthService, HttpClient, EventAggregator, LocalStorageBase)
export class UsersServiceBase extends HttpProxyBase {
  private readonly profileImageThumbRegex =
    /res.cloudinary.com\/personalassistant\/t_profile_thumbnail\/(development|production)\/users/g;

  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator,
    private readonly localStorageBase: LocalStorageBase
  ) {
    super(authService, httpClient, eventAggregator);
  }

  async getProfileImageUri(): Promise<string> {
    const currentSrc = this.localStorageBase.getProfileImageUri();

    if (!navigator.onLine) {
      return currentSrc;
    }

    const src = await this.ajax<string>("users/profile-image-uri");
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
  }
}

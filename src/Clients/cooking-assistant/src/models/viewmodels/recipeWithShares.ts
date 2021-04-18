import { SharingState } from "./sharingState";
import { Share } from "./share";

export class RecipeWithShares {
  constructor(
    public id: number,
    public name: string,
    public sharingState: SharingState,
    public ownerEmail: string,
    public ownerImageUri: string,
    public userShare: Share,
    public shares: Array<Share>
  ) {}
}

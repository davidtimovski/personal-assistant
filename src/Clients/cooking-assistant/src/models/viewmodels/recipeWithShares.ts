import { SharingState } from "./sharingState";
import { Share } from "./share";

export class RecipeWithShares {
  id: number;
  name: string;
  sharingState: SharingState;
  ownerEmail: string;
  ownerImageUri: string;
  userShare: Share;
  shares: Array<Share>;
}

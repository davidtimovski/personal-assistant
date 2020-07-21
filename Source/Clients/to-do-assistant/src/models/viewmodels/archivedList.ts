import { SharingState } from "./sharingState";

export class ArchivedList {
  constructor(
    public id: number,
    public name: string,
    public icon: string,
    public sharingState: SharingState
  ) {}
}

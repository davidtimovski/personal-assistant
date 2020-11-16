export class CostSummary {
  constructor(
    public isSet: boolean,
    public productSize: number,
    public productSizeIsOneUnit: boolean,
    public cost: number,
    public costPerServing: number,
    public currency: string,
    public ingredientIds: Array<number>
  ) {}
}

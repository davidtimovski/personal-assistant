export class PriceData {
  constructor(
    public isSet: boolean,
    public productSize: number,
    public productSizeIsOneUnit: boolean,
    public price: number,
    public currency: string
  ) {}
}

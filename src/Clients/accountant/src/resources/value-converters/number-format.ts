export class NumberFormatValueConverter {
  constructor() {}

  toView(value: any, currency: string) {
    if (isNaN(parseFloat(value))) {
      return null;
    }

    if (currency === "MKD") {
      return new Intl.NumberFormat("mk-MK", {
        maximumFractionDigits: 0
      }).format(value);
    }

    return new Intl.NumberFormat().format(value);
  }
}

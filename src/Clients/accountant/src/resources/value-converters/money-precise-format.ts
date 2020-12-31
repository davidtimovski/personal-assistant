export class MoneyPreciseFormatValueConverter {
  constructor() {}

  toView(value: any, currency: string) {
    if (isNaN(parseFloat(value))) {
      return null;
    }

    if (currency === "MKD") {
      const formatted = new Intl.NumberFormat("mk-MK", {
        maximumFractionDigits: 4,
      }).format(value);
      return formatted;
    }

    return new Intl.NumberFormat("de-DE", {
      style: "currency",
      maximumFractionDigits: 4,
    }).format(value);
  }
}

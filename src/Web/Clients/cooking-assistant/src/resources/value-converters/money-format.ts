export class MoneyFormatValueConverter {
  toView(value: any, currency: string) {
    if (isNaN(parseFloat(value))) {
      return null;
    }

    if (currency === "MKD") {
      const formatted = new Intl.NumberFormat("mk-MK", {
        maximumFractionDigits: 0,
      }).format(value);
      return formatted + " MKD";
    }

    return new Intl.NumberFormat("de-DE", {
      style: "currency",
      currency: currency,
    }).format(value);
  }
}

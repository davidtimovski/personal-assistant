export class MoneyFormattingHelper {
  static format(amount: number, currency: string): string {
    if (currency === "MKD") {
      const formatted = new Intl.NumberFormat("mk-MK", {
        maximumFractionDigits: 0,
      }).format(amount);
      return formatted + " MKD";
    }

    return new Intl.NumberFormat("de-DE", {
      style: "currency",
      currency: currency,
    }).format(amount);
  }
}

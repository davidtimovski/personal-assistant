export class MilligramsNumberFormatValueConverter {
  toView(value: any) {
    if (isNaN(parseFloat(value))) {
      return null;
    }

    if (value > 10) {
      return Math.round(value);
    }

    return new Intl.NumberFormat().format(value);
  }
}

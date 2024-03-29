export class GramsNumberFormatValueConverter {
  toView(value: any) {
    if (isNaN(parseFloat(value))) {
      return null;
    }

    return new Intl.NumberFormat().format(value);
  }
}

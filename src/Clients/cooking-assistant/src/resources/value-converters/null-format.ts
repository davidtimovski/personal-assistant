export class NullFormatValueConverter {
  fromView(value: string) {
    if (value === "") {
      return null;
    }

    return value;
  }
}

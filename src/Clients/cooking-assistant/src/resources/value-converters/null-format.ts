export class NullFormatValueConverter {
  constructor() {}

  fromView(value: string) {
    if (value === "") {
      return null;
    }

    return value;
  }
}

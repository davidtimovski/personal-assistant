export class Tooltip {
  public title: string;
  public question: string;
  public answer: string;

  constructor(public key: string, public isDismissed: boolean) {}
}

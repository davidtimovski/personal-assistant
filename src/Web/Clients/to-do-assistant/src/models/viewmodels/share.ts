export class Share {
  constructor(
    public userId: number,
    public email: string,
    public imageUri: string,
    public isAdmin: boolean,
    public isAccepted: boolean
  ) {}
}

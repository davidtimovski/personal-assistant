export class EditRecipeIngredient {
  constructor(
    public id: number,
    public taskId: number,
    public taskList: string,
    public name: string,
    public amount: string,
    public unit: string,
    public existing: boolean
  ) {}
}

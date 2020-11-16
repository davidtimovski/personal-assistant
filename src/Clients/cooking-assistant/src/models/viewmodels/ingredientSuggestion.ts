export class IngredientSuggestion {
  constructor(
    public id: number,
    public taskId: number,
    public name: string,
    public unit: string,
    public label: string,
    public group: string
  ) {}
}

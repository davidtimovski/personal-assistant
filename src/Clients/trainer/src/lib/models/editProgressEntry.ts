export class EditAmountProgressEntry {
	constructor(public id: number, public exerciseId: number, public date: string, public sets: AmountSets[]) {}
}

export class AmountSets {
	constructor(public set: number, public amount: number) {}
}

// @ts-ignore
export async function load({ params }) {
	return {
		isExpense: params.type === '0'
	};
}

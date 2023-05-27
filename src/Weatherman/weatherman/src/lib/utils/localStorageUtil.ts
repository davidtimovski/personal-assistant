import { LocalStorageBase } from '../../../../../Core/shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super(
			new Map<string, any>([
				[LocalStorageKeys.TemperatureUnit, 'celsius'],
				[LocalStorageKeys.PrecipitationUnit, 'mm'],
				[LocalStorageKeys.WindSpeedUnit, 'kmh']
			])
		);
	}
}

export enum LocalStorageKeys {
	TemperatureUnit = 'temperatureUnit',
	PrecipitationUnit = 'precipitationUnit',
	WindSpeedUnit = 'windSpeedUnit'
}

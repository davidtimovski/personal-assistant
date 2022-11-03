CREATE TABLE public.weatherman_forecasts
(
	id serial NOT NULL,
	latitude real NOT NULL,
	longitude real NOT NULL,
	temperature_unit character varying(10) COLLATE pg_catalog."default" NOT NULL,
	precipitation_unit character varying(6) COLLATE pg_catalog."default" NOT NULL,
	wind_speed_unit character varying(3) COLLATE pg_catalog."default" NOT NULL,
    last_update timestamp with time zone NOT NULL,
    data json NOT NULL,
    CONSTRAINT pk_weatherman_forecasts PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.weatherman_forecasts
    OWNER to personalassistant;

CREATE INDEX ix_weatherman_forecasts_latitude_longitude
    ON public.weatherman_forecasts USING btree
    (latitude,longitude)
    TABLESPACE pg_default;

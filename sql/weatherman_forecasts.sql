CREATE TABLE public.weatherman_forecasts
(
	id serial NOT NULL,
	latitude real NOT NULL,
	longitude real NOT NULL,
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

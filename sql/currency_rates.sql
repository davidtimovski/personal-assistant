CREATE TABLE public.currency_rates
(
    date date NOT NULL,
    rates json NOT NULL,
    CONSTRAINT pk_currency_rates PRIMARY KEY (date)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.currency_rates
    OWNER to personalassistant;

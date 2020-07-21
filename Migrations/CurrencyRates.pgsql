-- Table: public."CurrencyRates"

-- DROP TABLE public."CurrencyRates";

CREATE TABLE public."CurrencyRates"
(
    "Date" date NOT NULL,
    "Rates" json NOT NULL,
    CONSTRAINT "PK_CurrencyRates" PRIMARY KEY ("Date")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."CurrencyRates"
    OWNER to personalassistant;

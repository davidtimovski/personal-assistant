CREATE TABLE accountant.debts
(
    id serial NOT NULL,
    user_id integer NOT NULL,
    person character varying(20) COLLATE pg_catalog."default" NOT NULL,
    amount decimal(10, 2) NOT NULL,
    currency character varying(3) COLLATE pg_catalog."default" NOT NULL DEFAULT 'EUR',
    description character varying(5000) COLLATE pg_catalog."default",
    user_is_debtor boolean NOT NULL DEFAULT FALSE,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_debts PRIMARY KEY (id),
    CONSTRAINT fk_debts_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE accountant.debts
    OWNER to personalassistant;

CREATE INDEX ix_debts_user_id
    ON accountant.debts USING btree
    (user_id)
    TABLESPACE pg_default;

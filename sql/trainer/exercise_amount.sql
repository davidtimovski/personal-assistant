CREATE TABLE trainer.exercise_amount
(
    id serial NOT NULL,
    user_id integer NOT NULL,
    name character varying(50) NOT NULL COLLATE pg_catalog."default",
	sets integer NOT NULL,
    amount_unit character varying(50) NOT NULL COLLATE pg_catalog."default",
	created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_exercise_amount PRIMARY KEY (id),
    CONSTRAINT fk_exercise_amount_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE trainer.exercise_amount
    OWNER to personalassistant;

CREATE INDEX ix_exercise_amount_user_id
    ON trainer.exercise_amount USING btree
    (user_id)
    TABLESPACE pg_default;

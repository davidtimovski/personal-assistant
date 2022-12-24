CREATE TABLE trainer.progress_amount
(
    exercise_id integer NOT NULL,
	date date NOT NULL, 
	set integer NOT NULL,
	amount integer NOT NULL,
	created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
	CONSTRAINT pk_progress_amount PRIMARY KEY (exercise_id, date),
    CONSTRAINT fk_progress_amount_exercise_amount_exercise_id FOREIGN KEY (exercise_id)
    REFERENCES trainer.exercise_amount (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE trainer.progress_amount
    OWNER to personalassistant;

CREATE INDEX ix_progress_amount_exercise_id
    ON trainer.progress_amount USING btree
    (exercise_id)
    TABLESPACE pg_default;

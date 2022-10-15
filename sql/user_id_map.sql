CREATE TABLE public.user_id_map
(
    user_id integer NOT NULL,
    auth0_id character varying(50) NOT NULL COLLATE pg_catalog."default",
    CONSTRAINT pk_user_id_map PRIMARY KEY (user_id),
    CONSTRAINT fk_user_id_map_aspnetusers_user_id FOREIGN KEY (user_id)
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.user_id_map
    OWNER to personalassistant;

CREATE INDEX ix_user_id_map_user_id
    ON public.user_id_map USING btree
    (user_id)
    TABLESPACE pg_default;

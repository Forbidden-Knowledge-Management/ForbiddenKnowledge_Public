ALTER TABLE IF EXISTS public.external_user
ADD CONSTRAINT external_user_uppercased_pseudonym_key UNIQUE (uppercased_pseudonym);

ALTER TABLE IF EXISTS public.blog_post_comment
ADD user_id INTEGER;

UPDATE blog_post_comment
SET user_id = external_user.id
FROM external_user
WHERE blog_post_comment.pseudonym = external_user.original_pseudonym;

ALTER TABLE IF EXISTS public.blog_post_comment
ALTER COLUMN user_id SET NOT NULL;

ALTER TABLE IF EXISTS blog_post_comment
ADD CONSTRAINT blog_post_comment_user_id_fkey FOREIGN KEY (user_id)
REFERENCES public.external_user (id) MATCH SIMPLE
ON UPDATE NO ACTION
ON DELETE CASCADE;



--delete the blog_post_comment.pseudonym column
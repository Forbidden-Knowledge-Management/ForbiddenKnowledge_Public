ALTER TABLE IF EXISTS public.blog_post_tag_lookup
ADD tag_type VARCHAR(25);

UPDATE blog_post_tag_lookup
SET tag_type = 'Generic';

INSERT INTO public.blog_post_tag_lookup(
	id, name, tag_type)
	VALUES (4, 'Chapter 2: Health', 'FK_Excerpt');

ALTER TABLE IF EXISTS public.blog_post_tag_lookup
ALTER COLUMN tag_type SET NOT NULL;
21422_Maksymilian_Rawski.zip:
	mkdir -p projekt
	rm -rf projekt/*
	rsync -qa . --exclude projekt projekt/
	cd projekt && git clean -fx && rm -rf obj/ bin/ Properties/ .git .gitignore
	zip -r $@ projekt
	rm -rf projekt



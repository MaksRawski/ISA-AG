ZIP_NAME := 21422_Maksymilian_Rawski.zip

all:
	mkdir -p projekt
	rm -rf projekt/* ${ZIP_NAME}
	rsync -qa . --exclude projekt projekt/
	cd projekt && git clean -fx && rm -rf .vs/ obj/ bin/ Properties/ .git .gitignore LICENSE Makefile
	zip -r ${ZIP_NAME} projekt
	rm -rf projekt

.PHONY: all

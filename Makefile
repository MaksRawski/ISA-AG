NAME := 21422_Maksymilian_Rawski
PUBLISH_PATH := ISA/bin/Release/net6.0-windows/publish/win-x64/ISA.exe

all: $(NAME).zip $(NAME).exe

$(NAME).zip:
	rm -rf ISA_BACKUP
	cp -r ISA ISA_BACKUP
	cd ISA && git clean -fxd && rm -rf .gitignore \
		ISA.sln # references Tests which we won't include in the source code
	zip -r $@ ISA
	rm -rf ISA
	mv ISA_BACKUP ISA

$(NAME).exe: $(PUBLISH_PATH)
	cp $(PUBLISH_PATH) $@

.PHONY: all

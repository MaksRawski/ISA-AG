NAME := 21422_Maksymilian_Rawski
PUBLISH_PATH := ISA/UI/bin/Release/net6.0-windows/publish/win-x64
CODE_PATH := $(NAME)_Kod
CODE_PATH_ZIP := $(NAME)_Kod.zip
NAME_PROGRAM := $(NAME)_Program
NAME_PROGRAM_ZIP := $(NAME)_Program.zip

all: $(CODE_PATH_ZIP) $(NAME_PROGRAM_ZIP)

$(CODE_PATH_ZIP):
	rm -rf ISA_BACKUP
	cp -r ISA ISA_BACKUP
	cd ISA && git clean -fxd && rm -rf .gitignore Benchmark TestyCLI UnitTests
	sed -i '/Bench/d' ISA/ISA.sln
	sed -i '/TestyCLI/d' ISA/ISA.sln
	sed -i '/UnitTests/d' ISA/ISA.sln
	mv ISA $(CODE_PATH)
	zip -r $@ $(CODE_PATH)
	rm -rf $(CODE_PATH)
	mv ISA_BACKUP ISA


$(NAME_PROGRAM_ZIP): $(PUBLISH_PATH)
	cp -r $(PUBLISH_PATH) $(NAME_PROGRAM)
	zip -r $@ $(NAME_PROGRAM)/*
	rm -rf $(NAME_PROGRAM)
clean:
	rm -f $(CODE_PATH_ZIP) $(NAME_PROGRAM_ZIP)
.PHONY: all

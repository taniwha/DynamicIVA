KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
DIVAGAMEDATA  := ${GAMEDATA}/DynamicIVA
PLUGINDIR	:= ${DIVAGAMEDATA}/Plugins

TARGETS		:= DynamicIVA.dll

DIVA_FILES := \
	InternalModelSwitch.cs \
	NodeChecker.cs \
	PartModelSwitch.cs \
	$e

DOC_FILES := \
	../License.txt \
	$e

#	DynamicIVA.png
#	README.md

RESGEN2		:= resgen2
GMCS		:= gmcs
GMCSFLAGS	:= -optimize -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

all: version ${TARGETS} #DynamicIVA.png

.PHONY: version
version:
	@#./git-version.sh

info:
	@echo "DynamicIVA Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

DynamicIVA.dll: ${DIVA_FILES}
	${GMCS} ${GMCSFLAGS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine \
		-out:$@ $^

#DynamicIVA.png: DynamicIVA.svg
#	inkscape --export-png $@ $^

clean:
	rm -f ${TARGETS} AssemblyInfo.cs #DynamicIVA.png

install: all
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}
	#cp ${DOC_FILES} ${DIVAGAMEDATA}

.PHONY: all clean install

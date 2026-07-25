window.ltToeicTestTaking = (() => {
    const registrations = new Map();
    const stickyOffset = 110;

    function dispose(rootId) {
        const registration = registrations.get(rootId);
        if (!registration) {
            return;
        }

        window.removeEventListener("scroll", registration.onViewportChanged);
        window.removeEventListener("resize", registration.onViewportChanged);
        registration.audios.forEach(audio =>
            audio.removeEventListener("play", registration.onAudioPlayed));

        if (registration.animationFrame) {
            window.cancelAnimationFrame(registration.animationFrame);
        }

        registrations.delete(rootId);
    }

    function initialize(rootId, dotNetReference) {
        dispose(rootId);

        const root = document.getElementById(rootId);
        if (!root) {
            return;
        }

        const sections = Array.from(root.querySelectorAll("[data-test-part-id]"));
        const audios = Array.from(root.querySelectorAll("audio[data-part-audio]"));
        if (!sections.length) {
            return;
        }

        const registration = {
            sections,
            audios,
            dotNetReference,
            activePartId: null,
            animationFrame: 0,
            onViewportChanged: null,
            onAudioPlayed: null
        };

        const updateActivePart = () => {
            registration.animationFrame = 0;

            let activeSection = sections.find(section => {
                const rect = section.getBoundingClientRect();
                return rect.top <= stickyOffset && rect.bottom > stickyOffset;
            });

            if (!activeSection) {
                activeSection = sections.reduce((nearest, section) => {
                    if (!nearest) {
                        return section;
                    }

                    const currentDistance = Math.abs(
                        section.getBoundingClientRect().top - stickyOffset);
                    const nearestDistance = Math.abs(
                        nearest.getBoundingClientRect().top - stickyOffset);
                    return currentDistance < nearestDistance ? section : nearest;
                }, null);
            }

            const partId = activeSection?.dataset.testPartId;
            if (!partId || partId === registration.activePartId) {
                return;
            }

            registration.activePartId = partId;

            audios.forEach(audio => {
                if (audio.dataset.partAudio !== partId && !audio.paused) {
                    audio.pause();
                }
            });

            dotNetReference
                .invokeMethodAsync("SetActivePartFromScroll", Number(partId))
                .catch(() => dispose(rootId));
        };

        registration.onViewportChanged = () => {
            if (!registration.animationFrame) {
                registration.animationFrame = window.requestAnimationFrame(updateActivePart);
            }
        };

        registration.onAudioPlayed = event => {
            audios.forEach(audio => {
                if (audio !== event.currentTarget && !audio.paused) {
                    audio.pause();
                }
            });
        };

        window.addEventListener("scroll", registration.onViewportChanged, { passive: true });
        window.addEventListener("resize", registration.onViewportChanged);
        audios.forEach(audio =>
            audio.addEventListener("play", registration.onAudioPlayed));

        registrations.set(rootId, registration);
        updateActivePart();
    }

    return { initialize, dispose };
})();

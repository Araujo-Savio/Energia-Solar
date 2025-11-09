// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Sistema de busca de cidades brasileiras
class CitySearchManager {
    constructor() {
        this.cities = [];
        this.loadCities();
    }

    // Carrega as cidades do JSON
    async loadCities() {
        try {
            const response = await fetch('/js/cities.json');
            const data = await response.json();
            this.cities = data.cities;
        } catch (error) {
            console.error('Erro ao carregar cidades:', error);
            this.cities = [];
        }
    }

    // Busca cidades que correspondem ao termo
    searchCities(query) {
        if (!query || query.length < 2) return [];
        
        const normalizedQuery = this.normalizeString(query);
        
        return this.cities
            .filter(city => {
                const normalizedCity = this.normalizeString(city);
                return normalizedCity.includes(normalizedQuery);
            })
            .slice(0, 10) // Limita a 10 resultados
            .sort((a, b) => {
                // Prioriza cidades que começam com o termo buscado
                const aNormalized = this.normalizeString(a);
                const bNormalized = this.normalizeString(b);
                const queryNormalized = this.normalizeString(query);
                
                const aStartsWith = aNormalized.startsWith(queryNormalized);
                const bStartsWith = bNormalized.startsWith(queryNormalized);
                
                if (aStartsWith && !bStartsWith) return -1;
                if (!aStartsWith && bStartsWith) return 1;
                
                return a.localeCompare(b);
            });
    }

    // Remove acentos e normaliza string para busca
    normalizeString(str) {
        return str
            .toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '');
    }

    // Inicializa o autocomplete em um input
    initializeAutocomplete(inputElement) {
        if (!inputElement) return;

        const wrapper = document.createElement('div');
        wrapper.className = 'city-autocomplete-wrapper';
        wrapper.style.position = 'relative';

        // Move o input para dentro do wrapper
        inputElement.parentNode.insertBefore(wrapper, inputElement);
        wrapper.appendChild(inputElement);

        // Cria o dropdown de sugestões
        const dropdown = document.createElement('div');
        dropdown.className = 'city-autocomplete-dropdown';
        dropdown.style.cssText = `
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            background: white;
            border: 1px solid #ddd;
            border-top: none;
            max-height: 200px;
            overflow-y: auto;
            z-index: 1000;
            display: none;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            border-radius: 0 0 4px 4px;
        `;
        wrapper.appendChild(dropdown);

        let debounceTimer;
        let currentFocus = -1;

        // Event listener para input
        inputElement.addEventListener('input', (e) => {
            const query = e.target.value;
            
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                this.showSuggestions(query, dropdown, inputElement);
            }, 300);
        });

        // Event listener para navegação com teclado
        inputElement.addEventListener('keydown', (e) => {
            const items = dropdown.querySelectorAll('.city-suggestion-item');
            
            if (e.key === 'ArrowDown') {
                e.preventDefault();
                currentFocus = currentFocus < items.length - 1 ? currentFocus + 1 : 0;
                this.setActiveItem(items, currentFocus);
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                currentFocus = currentFocus > 0 ? currentFocus - 1 : items.length - 1;
                this.setActiveItem(items, currentFocus);
            } else if (e.key === 'Enter') {
                e.preventDefault();
                if (currentFocus >= 0 && items[currentFocus]) {
                    items[currentFocus].click();
                }
            } else if (e.key === 'Escape') {
                this.hideSuggestions(dropdown);
                currentFocus = -1;
            }
        });

        // Esconde sugestões quando clica fora
        document.addEventListener('click', (e) => {
            if (!wrapper.contains(e.target)) {
                this.hideSuggestions(dropdown);
                currentFocus = -1;
            }
        });

        // Esconde sugestões quando o input perde o foco
        inputElement.addEventListener('blur', (e) => {
            // Pequeno delay para permitir cliques nas sugestões
            setTimeout(() => {
                if (!wrapper.contains(document.activeElement)) {
                    this.hideSuggestions(dropdown);
                    currentFocus = -1;
                }
            }, 150);
        });
    }

    // Mostra as sugestões
    showSuggestions(query, dropdown, inputElement) {
        const suggestions = this.searchCities(query);
        
        if (suggestions.length === 0 || !query.trim()) {
            this.hideSuggestions(dropdown);
            return;
        }

        dropdown.innerHTML = '';
        
        suggestions.forEach((city, index) => {
            const item = document.createElement('div');
            item.className = 'city-suggestion-item';
            item.style.cssText = `
                padding: 12px 16px;
                cursor: pointer;
                border-bottom: 1px solid #f0f0f0;
                transition: background-color 0.2s ease;
                display: flex;
                align-items: center;
            `;
            
            // Destaca o termo pesquisado
            const highlightedCity = this.highlightMatch(city, query);
            
            item.innerHTML = `
                <i class="fas fa-map-marker-alt text-muted me-2"></i>
                <span>${highlightedCity}</span>
            `;

            // Event listeners para o item
            item.addEventListener('mouseenter', () => {
                this.setActiveItem(dropdown.querySelectorAll('.city-suggestion-item'), index);
            });

            item.addEventListener('click', () => {
                inputElement.value = city;
                this.hideSuggestions(dropdown);
                
                // Dispara evento de mudança
                const changeEvent = new Event('change', { bubbles: true });
                inputElement.dispatchEvent(changeEvent);
            });

            dropdown.appendChild(item);
        });

        dropdown.style.display = 'block';
    }

    // Esconde as sugestões
    hideSuggestions(dropdown) {
        dropdown.style.display = 'none';
    }

    // Define o item ativo
    setActiveItem(items, index) {
        items.forEach((item, i) => {
            if (i === index) {
                item.style.backgroundColor = '#f8f9fa';
                item.style.color = '#495057';
            } else {
                item.style.backgroundColor = '';
                item.style.color = '';
            }
        });
    }

    // Destaca o termo pesquisado na sugestão
    highlightMatch(city, query) {
        if (!query) return city;
        
        const regex = new RegExp(`(${query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
        return city.replace(regex, '<strong>$1</strong>');
    }
}

// Inicializar o gerenciador de cidades quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function() {
    const cityManager = new CitySearchManager();
    
    // Aguarda um pouco para as cidades carregarem
    setTimeout(() => {
        // Inicializa autocomplete em todos os campos de localização
        const locationInputs = document.querySelectorAll('input[name="Location"], input[id*="Location"], input[data-city-search]');
        locationInputs.forEach(input => {
            cityManager.initializeAutocomplete(input);
        });
    }, 500);
});

// CSS adicional para melhorar a aparência
const additionalCSS = `
.city-autocomplete-wrapper .form-control {
    border-radius: 4px 4px 0 0;
}

.city-autocomplete-wrapper .form-control:focus + .city-autocomplete-dropdown {
    border-color: #80bdff;
}

.city-suggestion-item:hover {
    background-color: #e9ecef !important;
}

.city-suggestion-item:last-child {
    border-bottom: none;
}

.city-autocomplete-dropdown::-webkit-scrollbar {
    width: 8px;
}

.city-autocomplete-dropdown::-webkit-scrollbar-track {
    background: #f1f1f1;
}

.city-autocomplete-dropdown::-webkit-scrollbar-thumb {
    background: #c1c1c1;
    border-radius: 4px;
}

.city-autocomplete-dropdown::-webkit-scrollbar-thumb:hover {
    background: #a8a8a8;
}
`;

// Adiciona o CSS ao documento
const style = document.createElement('style');
style.textContent = additionalCSS;
document.head.appendChild(style);

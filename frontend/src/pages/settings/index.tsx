import { FiSave, FiCreditCard, FiMail, FiLock, FiGlobe, FiBell, FiUser, FiShield } from 'react-icons/fi';
import { MainLayout } from '@/components/layout/MainLayout';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Switch } from '@/components/ui/switch';

type ProfileFormData = {
  name: string;
  email: string;
  phone: string;
  bio: string;
};

const SettingsPage = () => {
  // Dados de exemplo
  const profileData: ProfileFormData = {
    name: 'João Silva',
    email: 'joao.silva@exemplo.com',
    phone: '(11) 98765-4321',
    bio: 'Gerente de Vendas na Empresa XYZ',
  };

  const notificationSettings = {
    email: true,
    push: true,
    weeklyDigest: true,
    productUpdates: false,
  };

  return (
    <MainLayout>
      <div className="p-6">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Configurações</h1>
          <p className="text-gray-500">Gerencie as configurações da sua conta e preferências</p>
        </div>

        <Tabs defaultValue="profile" className="w-full">
          <TabsList className="grid w-full md:w-auto grid-cols-2 md:grid-cols-4 mb-6">
            <TabsTrigger value="profile" className="flex items-center gap-2">
              <FiUser className="h-4 w-4" />
              <span className="hidden sm:inline">Perfil</span>
            </TabsTrigger>
            <TabsTrigger value="security" className="flex items-center gap-2">
              <FiShield className="h-4 w-4" />
              <span className="hidden sm:inline">Segurança</span>
            </TabsTrigger>
            <TabsTrigger value="notifications" className="flex items-center gap-2">
              <FiBell className="h-4 w-4" />
              <span className="hidden sm:inline">Notificações</span>
            </TabsTrigger>
            <TabsTrigger value="billing" className="flex items-center gap-2">
              <FiCreditCard className="h-4 w-4" />
              <span className="hidden sm:inline">Pagamento</span>
            </TabsTrigger>
          </TabsList>

          {/* Aba de Perfil */}
          <TabsContent value="profile" className="space-y-6">
            <div className="bg-white p-6 rounded-lg border border-gray-100 shadow-sm">
              <h2 className="text-lg font-semibold mb-6">Informações do Perfil</h2>
              <form className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-2">
                    <Label htmlFor="name">Nome</Label>
                    <Input id="name" defaultValue={profileData.name} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="email">E-mail</Label>
                    <Input id="email" type="email" defaultValue={profileData.email} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="phone">Telefone</Label>
                    <Input id="phone" defaultValue={profileData.phone} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="bio">Biografia</Label>
                    <Input id="bio" defaultValue={profileData.bio} />
                  </div>
                </div>
                <div className="flex justify-end">
                  <Button type="submit" className="gap-2">
                    <FiSave className="h-4 w-4" />
                    Salvar alterações
                  </Button>
                </div>
              </form>
            </div>
          </TabsContent>

          {/* Aba de Segurança */}
          <TabsContent value="security" className="space-y-6">
            <div className="bg-white p-6 rounded-lg border border-gray-100 shadow-sm">
              <h2 className="text-lg font-semibold mb-6">Segurança da Conta</h2>
              <div className="space-y-6">
                <div className="flex flex-col sm:flex-row sm:items-center justify-between p-4 bg-gray-50 rounded-lg">
                  <div>
                    <h3 className="font-medium">Alterar Senha</h3>
                    <p className="text-sm text-gray-500">Atualize sua senha regularmente para manter sua conta segura</p>
                  </div>
                  <Button variant="outline" className="mt-2 sm:mt-0">
                    Alterar Senha
                  </Button>
                </div>

                <div className="flex flex-col sm:flex-row sm:items-center justify-between p-4 bg-gray-50 rounded-lg">
                  <div>
                    <h3 className="font-medium">Autenticação de Dois Fatores (2FA)</h3>
                    <p className="text-sm text-gray-500">Adicione uma camada extra de segurança à sua conta</p>
                  </div>
                  <div className="flex items-center mt-2 sm:mt-0">
                    <span className="text-sm text-gray-500 mr-3">Desativado</span>
                    <Button variant="outline">
                      Ativar 2FA
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          </TabsContent>

          {/* Aba de Notificações */}
          <TabsContent value="notifications" className="space-y-6">
            <div className="bg-white p-6 rounded-lg border border-gray-100 shadow-sm">
              <h2 className="text-lg font-semibold mb-6">Preferências de Notificação</h2>
              <div className="space-y-6">
                <div className="flex items-center justify-between">
                  <div className="space-y-1">
                    <h3 className="font-medium">Notificações por E-mail</h3>
                    <p className="text-sm text-gray-500">Receba atualizações importantes por e-mail</p>
                  </div>
                  <Switch id="email-notifications" defaultChecked={notificationSettings.email} />
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-1">
                    <h3 className="font-medium">Notificações Push</h3>
                    <p className="text-sm text-gray-500">Receba notificações no seu dispositivo</p>
                  </div>
                  <Switch id="push-notifications" defaultChecked={notificationSettings.push} />
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-1">
                    <h3 className="font-medium">Resumo Semanal</h3>
                    <p className="text-sm text-gray-500">Receba um resumo semanal das suas atividades</p>
                  </div>
                  <Switch id="weekly-digest" defaultChecked={notificationSettings.weeklyDigest} />
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-1">
                    <h3 className="font-medium">Atualizações de Produtos</h3>
                    <p className="text-sm text-gray-500">Seja notificado sobre novos recursos e atualizações</p>
                  </div>
                  <Switch id="product-updates" defaultChecked={notificationSettings.productUpdates} />
                </div>
              </div>
            </div>
          </TabsContent>

          {/* Aba de Pagamento */}
          <TabsContent value="billing" className="space-y-6">
            <div className="bg-white p-6 rounded-lg border border-gray-100 shadow-sm">
              <h2 className="text-lg font-semibold mb-6">Métodos de Pagamento</h2>
              <div className="space-y-6">
                <div className="p-4 border border-gray-200 rounded-lg">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center">
                      <FiCreditCard className="h-6 w-6 text-gray-500 mr-3" />
                      <div>
                        <h3 className="font-medium">Cartão de Crédito</h3>
                        <p className="text-sm text-gray-500">•••• •••• •••• 4242</p>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      Alterar
                    </Button>
                  </div>
                </div>

                <div className="p-4 border border-gray-200 rounded-lg bg-gray-50">
                  <h3 className="font-medium mb-2">Plano Atual</h3>
                  <div className="flex items-baseline">
                    <span className="text-2xl font-bold">R$ 99</span>
                    <span className="text-gray-500 ml-1">/mês</span>
                  </div>
                  <p className="text-sm text-gray-500 mt-1">Plano Profissional</p>
                  <Button variant="outline" className="mt-4">
                    Atualizar Plano
                  </Button>
                </div>
              </div>
            </div>
          </TabsContent>
        </Tabs>
      </div>
    </MainLayout>
  );
};

export default SettingsPage;

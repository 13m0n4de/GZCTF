import { FC } from 'react'
import { Anchor, Badge, Center, Group, HoverCard, Stack, Text, Title } from '@mantine/core'
import WithNavBar from '@Components/WithNavbar'
import MainIcon from '@Components/icon/MainIcon'
import { useLogoStyles } from '@Utils/ThemeOverride'
import { useConfig, ValidatedRepoMeta } from '@Utils/useConfig'
import { usePageTitle } from '@Utils/usePageTitle'

const About: FC = () => {
  const { classes, theme } = useLogoStyles()
  const { config } = useConfig()
  const { repo, valid, tag, sha, buildtime } = ValidatedRepoMeta()

  usePageTitle('关于')

  return (
    <WithNavBar>
      <Stack justify="space-between" h="calc(100vh - 16px)">
        <Center h="calc(100vh - 16px)">
          <Title order={2} className={classes.watermark}>
            GZ::CTF
          </Title>
          <Text className={classes.bio}>
            &gt; {config?.slogan ?? 'Hack for fun not for profit'}
            <Text span className={classes.blink}>
              _
            </Text>
          </Text>
        </Center>
        <Group position="right">
          <HoverCard shadow="md" position="top-end" withArrow openDelay={200} closeDelay={400}>
            <HoverCard.Target>
              <Badge
                onClick={() => window.open(repo, '_blank')}
                style={{
                  cursor: 'pointer',
                }}
                size="lg"
                variant="outline"
              >
                © 2022-Now GZTime {valid ? `#${sha.substring(0, 6)}` : ''}
              </Badge>
            </HoverCard.Target>
            <HoverCard.Dropdown>
              <Stack>
                <Group>
                  <MainIcon style={{ maxWidth: 60, height: 'auto' }} />
                  <Stack spacing="xs">
                    <Title
                      style={{
                        marginLeft: '-20px',
                        marginBottom: '-5px',
                      }}
                      className={classes.title}
                    >
                      GZ<span className={classes.brand}>::</span>CTF
                    </Title>
                    <Group ml="-18px" mt="-5px">
                      <Anchor
                        href="https://github.com/GZTimeWalker"
                        color="dimmed"
                        size="sm"
                        fw={500}
                        lh={1}
                      >
                        @GZTimeWalker
                      </Anchor>
                      <Badge
                        variant="gradient"
                        gradient={
                          valid
                            ? { from: 'teal', to: 'blue', deg: 60 }
                            : { from: 'red', to: 'orange', deg: -60 }
                        }
                        size="xs"
                      >
                        {valid ? `${tag}#${sha.substring(0, 6)}` : 'UNOFFICIAL'}
                      </Badge>
                    </Group>
                  </Stack>
                </Group>
                <Group spacing="xs">
                  <Text size="xs" fw={500} color="dimmed" ff={theme.fontFamilyMonospace}>
                    {valid
                      ? `Built at ${buildtime.format('YYYY-MM-DDTHH:mm:ssZ')}`
                      : 'This release is not officially built'}
                  </Text>
                </Group>
              </Stack>
            </HoverCard.Dropdown>
          </HoverCard>
        </Group>
      </Stack>
    </WithNavBar>
  )
}

export default About
